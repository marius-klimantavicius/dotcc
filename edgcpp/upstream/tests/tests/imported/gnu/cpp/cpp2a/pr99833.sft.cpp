//type: fp
//options: --c++20
# 0 "./cpp2a/pr99833.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./cpp2a/pr99833.C"



# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 1 3
# 40 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 1 3
# 60 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 3
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
# 941 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/pstl/pstl_config.h" 1 3
# 942 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++config.h" 2 3



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
# 111 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<bool __v>
    using __bool_constant = integral_constant<bool, __v>;



  using true_type = __bool_constant<true>;


  using false_type = __bool_constant<false>;




  template<bool __v>
    using bool_constant = __bool_constant<__v>;






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





  template<typename... _Bn>
    inline constexpr bool __or_v = __or_<_Bn...>::value;
  template<typename... _Bn>
    inline constexpr bool __and_v = __and_<_Bn...>::value;

  namespace __detail
  {
    template<typename , typename _B1, typename... _Bn>
      struct __disjunction_impl
      { using type = _B1; };

    template<typename _B1, typename _B2, typename... _Bn>
      struct __disjunction_impl<__enable_if_t<!bool(_B1::value)>, _B1, _B2, _Bn...>
      { using type = typename __disjunction_impl<void, _B2, _Bn...>::type; };

    template<typename , typename _B1, typename... _Bn>
      struct __conjunction_impl
      { using type = _B1; };

    template<typename _B1, typename _B2, typename... _Bn>
      struct __conjunction_impl<__enable_if_t<bool(_B1::value)>, _B1, _B2, _Bn...>
      { using type = typename __conjunction_impl<void, _B2, _Bn...>::type; };
  }


  template<typename... _Bn>
    struct conjunction
    : __detail::__conjunction_impl<void, _Bn...>::type
    { };

  template<>
    struct conjunction<>
    : true_type
    { };

  template<typename... _Bn>
    struct disjunction
    : __detail::__disjunction_impl<void, _Bn...>::type
    { };

  template<>
    struct disjunction<>
    : false_type
    { };

  template<typename _Pp>
    struct negation
    : __not_<_Pp>::type
    { };




  template<typename... _Bn>
    inline constexpr bool conjunction_v = conjunction<_Bn...>::value;

  template<typename... _Bn>
    inline constexpr bool disjunction_v = disjunction<_Bn...>::value;

  template<typename _Pp>
    inline constexpr bool negation_v = negation<_Pp>::value;





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
    struct __is_integral_helper<char8_t>
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
    __attribute__ ((__deprecated__ ("use '" "is_standard_layout && is_trivial" "' instead")))
    is_pod
    : public __bool_constant<__is_pod(_Tp)>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };





  template<typename _Tp>
    struct
    [[__deprecated__]]
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


  template<typename _Tp>
    constexpr bool __is_implicitly_default_constructible_v
      = requires (void(&__f)(_Tp)) { __f({}); };

  template<typename _Tp>
    struct __is_implicitly_default_constructible
    : __bool_constant<__is_implicitly_default_constructible_v<_Tp>>
    { };
# 1402 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
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





  template<typename _From, typename _To>
    inline constexpr bool is_nothrow_convertible_v
      = __is_nothrow_convertible(_From, _To);


  template<typename _From, typename _To>
    struct is_nothrow_convertible
    : public bool_constant<is_nothrow_convertible_v<_From, _To>>
    { };
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
    inline constexpr bool __is_nothrow_new_constructible
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


  template<>
    struct __make_unsigned<char8_t>
    {
      using __type
 = typename __make_unsigned_selector<char8_t, false, true>::__type;
    };


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


  template<>
    struct __make_signed<char8_t>
    {
      using __type
 = typename __make_signed_selector<char8_t, false, true>::__type;
    };


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




    template<typename _Tp, typename _Up>
      static __success_type<__remove_cvref_t<__cond_t<const _Tp&, const _Up&>>>
      _S_test_2(int);


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
    { } __attribute__ ((__deprecated__ ("use '" "std::invoke_result" "' instead")));


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




  template<typename...> using void_t = void;
# 2867 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Def, template<typename...> class _Op, typename... _Args>
    struct __detected_or
    {
      using type = _Def;
      using __is_detected = false_type;
    };


  template<typename _Def, template<typename...> class _Op, typename... _Args>
    requires requires { typename _Op<_Args...>; }
    struct __detected_or<_Def, _Op, _Args...>
    {
      using type = _Op<_Args...>;
      using __is_detected = true_type;
    };
# 2907 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
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
    constexpr
    inline
    _Require<__not_<__is_tuple_like<_Tp>>,
      is_move_constructible<_Tp>,
      is_move_assignable<_Tp>>
    swap(_Tp&, _Tp&)
    noexcept(__and_<is_nothrow_move_constructible<_Tp>,
             is_nothrow_move_assignable<_Tp>>::value);

  template<typename _Tp, size_t _Nm>
    constexpr
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






  template<typename _Tp>
    struct is_swappable
    : public __is_swappable_impl<_Tp>::type
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };


  template<typename _Tp>
    struct is_nothrow_swappable
    : public __is_nothrow_swappable_impl<_Tp>::type
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };



  template<typename _Tp>
    inline constexpr bool is_swappable_v =
      is_swappable<_Tp>::value;


  template<typename _Tp>
    inline constexpr bool is_nothrow_swappable_v =
      is_nothrow_swappable<_Tp>::value;



  namespace __swappable_with_details {
    using std::swap;

    struct __do_is_swappable_with_impl
    {
      template<typename _Tp, typename _Up, typename
               = decltype(swap(std::declval<_Tp>(), std::declval<_Up>())),
               typename
               = decltype(swap(std::declval<_Up>(), std::declval<_Tp>()))>
        static true_type __test(int);

      template<typename, typename>
        static false_type __test(...);
    };

    struct __do_is_nothrow_swappable_with_impl
    {
      template<typename _Tp, typename _Up>
        static __bool_constant<
          noexcept(swap(std::declval<_Tp>(), std::declval<_Up>()))
          &&
          noexcept(swap(std::declval<_Up>(), std::declval<_Tp>()))
        > __test(int);

      template<typename, typename>
        static false_type __test(...);
    };

  }

  template<typename _Tp, typename _Up>
    struct __is_swappable_with_impl
    : public __swappable_with_details::__do_is_swappable_with_impl
    {
      using type = decltype(__test<_Tp, _Up>(0));
    };


  template<typename _Tp>
    struct __is_swappable_with_impl<_Tp&, _Tp&>
    : public __swappable_details::__do_is_swappable_impl
    {
      using type = decltype(__test<_Tp&>(0));
    };

  template<typename _Tp, typename _Up>
    struct __is_nothrow_swappable_with_impl
    : public __swappable_with_details::__do_is_nothrow_swappable_with_impl
    {
      using type = decltype(__test<_Tp, _Up>(0));
    };


  template<typename _Tp>
    struct __is_nothrow_swappable_with_impl<_Tp&, _Tp&>
    : public __swappable_details::__do_is_nothrow_swappable_impl
    {
      using type = decltype(__test<_Tp&>(0));
    };



  template<typename _Tp, typename _Up>
    struct is_swappable_with
    : public __is_swappable_with_impl<_Tp, _Up>::type
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "first template argument must be a complete class or an unbounded array");
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Up>{}),
 "second template argument must be a complete class or an unbounded array");
    };


  template<typename _Tp, typename _Up>
    struct is_nothrow_swappable_with
    : public __is_nothrow_swappable_with_impl<_Tp, _Up>::type
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "first template argument must be a complete class or an unbounded array");
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Up>{}),
 "second template argument must be a complete class or an unbounded array");
    };



  template<typename _Tp, typename _Up>
    inline constexpr bool is_swappable_with_v =
      is_swappable_with<_Tp, _Up>::value;


  template<typename _Tp, typename _Up>
    inline constexpr bool is_nothrow_swappable_with_v =
      is_nothrow_swappable_with<_Tp, _Up>::value;
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




  template<typename _Functor, typename... _ArgTypes>
    struct invoke_result
    : public __invoke_result<_Functor, _ArgTypes...>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Functor>{}),
 "_Functor must be a complete class or an unbounded array");
      static_assert((std::__is_complete_or_unbounded(
 __type_identity<_ArgTypes>{}) && ...),
 "each argument type must be a complete class or an unbounded array");
    };


  template<typename _Fn, typename... _Args>
    using invoke_result_t = typename invoke_result<_Fn, _Args...>::type;


  template<typename _Fn, typename... _ArgTypes>
    struct is_invocable

    : public __bool_constant<__is_invocable(_Fn, _ArgTypes...)>



    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Fn>{}),
 "_Fn must be a complete class or an unbounded array");
      static_assert((std::__is_complete_or_unbounded(
 __type_identity<_ArgTypes>{}) && ...),
 "each argument type must be a complete class or an unbounded array");
    };


  template<typename _Ret, typename _Fn, typename... _ArgTypes>
    struct is_invocable_r
    : __is_invocable_impl<__invoke_result<_Fn, _ArgTypes...>, _Ret>::type
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Fn>{}),
 "_Fn must be a complete class or an unbounded array");
      static_assert((std::__is_complete_or_unbounded(
 __type_identity<_ArgTypes>{}) && ...),
 "each argument type must be a complete class or an unbounded array");
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Ret>{}),
 "_Ret must be a complete class or an unbounded array");
    };


  template<typename _Fn, typename... _ArgTypes>
    struct is_nothrow_invocable

    : public __bool_constant<__is_nothrow_invocable(_Fn, _ArgTypes...)>




    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Fn>{}),
 "_Fn must be a complete class or an unbounded array");
      static_assert((std::__is_complete_or_unbounded(
 __type_identity<_ArgTypes>{}) && ...),
 "each argument type must be a complete class or an unbounded array");
    };





  template<typename _Result, typename _Ret>
    using __is_nt_invocable_impl
      = typename __is_invocable_impl<_Result, _Ret>::__nothrow_conv;



  template<typename _Ret, typename _Fn, typename... _ArgTypes>
    struct is_nothrow_invocable_r
    : __and_<__is_nt_invocable_impl<__invoke_result<_Fn, _ArgTypes...>, _Ret>,
             __call_is_nothrow_<_Fn, _ArgTypes...>>::type
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Fn>{}),
 "_Fn must be a complete class or an unbounded array");
      static_assert((std::__is_complete_or_unbounded(
 __type_identity<_ArgTypes>{}) && ...),
 "each argument type must be a complete class or an unbounded array");
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Ret>{}),
 "_Ret must be a complete class or an unbounded array");
    };
# 3384 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
template <typename _Tp>
  inline constexpr bool is_void_v = is_void<_Tp>::value;
template <typename _Tp>
  inline constexpr bool is_null_pointer_v = is_null_pointer<_Tp>::value;
template <typename _Tp>
  inline constexpr bool is_integral_v = is_integral<_Tp>::value;
template <typename _Tp>
  inline constexpr bool is_floating_point_v = is_floating_point<_Tp>::value;


template <typename _Tp>
  inline constexpr bool is_array_v = __is_array(_Tp);
# 3406 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
template <typename _Tp>
  inline constexpr bool is_pointer_v = __is_pointer(_Tp);
# 3421 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
template <typename _Tp>
  inline constexpr bool is_lvalue_reference_v = false;
template <typename _Tp>
  inline constexpr bool is_lvalue_reference_v<_Tp&> = true;
template <typename _Tp>
  inline constexpr bool is_rvalue_reference_v = false;
template <typename _Tp>
  inline constexpr bool is_rvalue_reference_v<_Tp&&> = true;


template <typename _Tp>
  inline constexpr bool is_member_object_pointer_v =
    __is_member_object_pointer(_Tp);







template <typename _Tp>
  inline constexpr bool is_member_function_pointer_v =
    __is_member_function_pointer(_Tp);






template <typename _Tp>
  inline constexpr bool is_enum_v = __is_enum(_Tp);
template <typename _Tp>
  inline constexpr bool is_union_v = __is_union(_Tp);
template <typename _Tp>
  inline constexpr bool is_class_v = __is_class(_Tp);



template <typename _Tp>
  inline constexpr bool is_reference_v = __is_reference(_Tp);
# 3470 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
template <typename _Tp>
  inline constexpr bool is_arithmetic_v = is_arithmetic<_Tp>::value;
template <typename _Tp>
  inline constexpr bool is_fundamental_v = is_fundamental<_Tp>::value;


template <typename _Tp>
  inline constexpr bool is_object_v = __is_object(_Tp);





template <typename _Tp>
  inline constexpr bool is_scalar_v = is_scalar<_Tp>::value;
template <typename _Tp>
  inline constexpr bool is_compound_v = !is_fundamental_v<_Tp>;


template <typename _Tp>
  inline constexpr bool is_member_pointer_v = __is_member_pointer(_Tp);






template <typename _Tp>
  inline constexpr bool is_const_v = __is_const(_Tp);
# 3507 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
template <typename _Tp>
  inline constexpr bool is_function_v = __is_function(_Tp);
# 3519 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
template <typename _Tp>
  inline constexpr bool is_volatile_v = __is_volatile(_Tp);







template <typename _Tp>
 
  inline constexpr bool is_trivial_v = __is_trivial(_Tp);
template <typename _Tp>
  inline constexpr bool is_trivially_copyable_v = __is_trivially_copyable(_Tp);
template <typename _Tp>
  inline constexpr bool is_standard_layout_v = __is_standard_layout(_Tp);
template <typename _Tp>
  __attribute__ ((__deprecated__ ("use '" "is_standard_layout_v && is_trivial_v" "' instead")))
  inline constexpr bool is_pod_v = __is_pod(_Tp);
template <typename _Tp>
  [[__deprecated__]]
  inline constexpr bool is_literal_type_v = __is_literal_type(_Tp);
template <typename _Tp>
  inline constexpr bool is_empty_v = __is_empty(_Tp);
template <typename _Tp>
  inline constexpr bool is_polymorphic_v = __is_polymorphic(_Tp);
template <typename _Tp>
  inline constexpr bool is_abstract_v = __is_abstract(_Tp);
template <typename _Tp>
  inline constexpr bool is_final_v = __is_final(_Tp);

template <typename _Tp>
  inline constexpr bool is_signed_v = is_signed<_Tp>::value;
template <typename _Tp>
  inline constexpr bool is_unsigned_v = is_unsigned<_Tp>::value;

template <typename _Tp, typename... _Args>
  inline constexpr bool is_constructible_v = __is_constructible(_Tp, _Args...);
template <typename _Tp>
  inline constexpr bool is_default_constructible_v = __is_constructible(_Tp);
template <typename _Tp>
  inline constexpr bool is_copy_constructible_v
    = __is_constructible(_Tp, __add_lval_ref_t<const _Tp>);
template <typename _Tp>
  inline constexpr bool is_move_constructible_v
    = __is_constructible(_Tp, __add_rval_ref_t<_Tp>);

template <typename _Tp, typename _Up>
  inline constexpr bool is_assignable_v = __is_assignable(_Tp, _Up);
template <typename _Tp>
  inline constexpr bool is_copy_assignable_v
    = __is_assignable(__add_lval_ref_t<_Tp>, __add_lval_ref_t<const _Tp>);
template <typename _Tp>
  inline constexpr bool is_move_assignable_v
    = __is_assignable(__add_lval_ref_t<_Tp>, __add_rval_ref_t<_Tp>);

template <typename _Tp>
  inline constexpr bool is_destructible_v = is_destructible<_Tp>::value;

template <typename _Tp, typename... _Args>
  inline constexpr bool is_trivially_constructible_v
    = __is_trivially_constructible(_Tp, _Args...);
template <typename _Tp>
  inline constexpr bool is_trivially_default_constructible_v
    = __is_trivially_constructible(_Tp);
template <typename _Tp>
  inline constexpr bool is_trivially_copy_constructible_v
    = __is_trivially_constructible(_Tp, __add_lval_ref_t<const _Tp>);
template <typename _Tp>
  inline constexpr bool is_trivially_move_constructible_v
    = __is_trivially_constructible(_Tp, __add_rval_ref_t<_Tp>);

template <typename _Tp, typename _Up>
  inline constexpr bool is_trivially_assignable_v
    = __is_trivially_assignable(_Tp, _Up);
template <typename _Tp>
  inline constexpr bool is_trivially_copy_assignable_v
    = __is_trivially_assignable(__add_lval_ref_t<_Tp>,
    __add_lval_ref_t<const _Tp>);
template <typename _Tp>
  inline constexpr bool is_trivially_move_assignable_v
    = __is_trivially_assignable(__add_lval_ref_t<_Tp>,
    __add_rval_ref_t<_Tp>);


template <typename _Tp>
  inline constexpr bool is_trivially_destructible_v = false;

template <typename _Tp>
  requires (!is_reference_v<_Tp>) && requires (_Tp& __t) { __t.~_Tp(); }
  inline constexpr bool is_trivially_destructible_v<_Tp>
    = __has_trivial_destructor(_Tp);
template <typename _Tp>
  inline constexpr bool is_trivially_destructible_v<_Tp&> = true;
template <typename _Tp>
  inline constexpr bool is_trivially_destructible_v<_Tp&&> = true;
template <typename _Tp, size_t _Nm>
  inline constexpr bool is_trivially_destructible_v<_Tp[_Nm]>
    = is_trivially_destructible_v<_Tp>;






template <typename _Tp, typename... _Args>
  inline constexpr bool is_nothrow_constructible_v
    = __is_nothrow_constructible(_Tp, _Args...);
template <typename _Tp>
  inline constexpr bool is_nothrow_default_constructible_v
    = __is_nothrow_constructible(_Tp);
template <typename _Tp>
  inline constexpr bool is_nothrow_copy_constructible_v
    = __is_nothrow_constructible(_Tp, __add_lval_ref_t<const _Tp>);
template <typename _Tp>
  inline constexpr bool is_nothrow_move_constructible_v
    = __is_nothrow_constructible(_Tp, __add_rval_ref_t<_Tp>);

template <typename _Tp, typename _Up>
  inline constexpr bool is_nothrow_assignable_v
    = __is_nothrow_assignable(_Tp, _Up);
template <typename _Tp>
  inline constexpr bool is_nothrow_copy_assignable_v
    = __is_nothrow_assignable(__add_lval_ref_t<_Tp>,
         __add_lval_ref_t<const _Tp>);
template <typename _Tp>
  inline constexpr bool is_nothrow_move_assignable_v
    = __is_nothrow_assignable(__add_lval_ref_t<_Tp>, __add_rval_ref_t<_Tp>);

template <typename _Tp>
  inline constexpr bool is_nothrow_destructible_v =
    is_nothrow_destructible<_Tp>::value;

template <typename _Tp>
  inline constexpr bool has_virtual_destructor_v
    = __has_virtual_destructor(_Tp);

template <typename _Tp>
  inline constexpr size_t alignment_of_v = alignment_of<_Tp>::value;


template <typename _Tp>
  inline constexpr size_t rank_v = __array_rank(_Tp);
# 3671 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
template <typename _Tp, unsigned _Idx = 0>
  inline constexpr size_t extent_v = 0;
template <typename _Tp, size_t _Size>
  inline constexpr size_t extent_v<_Tp[_Size], 0> = _Size;
template <typename _Tp, unsigned _Idx, size_t _Size>
  inline constexpr size_t extent_v<_Tp[_Size], _Idx> = extent_v<_Tp, _Idx - 1>;
template <typename _Tp>
  inline constexpr size_t extent_v<_Tp[], 0> = 0;
template <typename _Tp, unsigned _Idx>
  inline constexpr size_t extent_v<_Tp[], _Idx> = extent_v<_Tp, _Idx - 1>;


template <typename _Tp, typename _Up>
  inline constexpr bool is_same_v = __is_same(_Tp, _Up);






template <typename _Base, typename _Derived>
  inline constexpr bool is_base_of_v = __is_base_of(_Base, _Derived);





template <typename _From, typename _To>
  inline constexpr bool is_convertible_v = __is_convertible(_From, _To);




template<typename _Fn, typename... _Args>
  inline constexpr bool is_invocable_v = is_invocable<_Fn, _Args...>::value;
template<typename _Fn, typename... _Args>
  inline constexpr bool is_nothrow_invocable_v
    = is_nothrow_invocable<_Fn, _Args...>::value;
template<typename _Ret, typename _Fn, typename... _Args>
  inline constexpr bool is_invocable_r_v
    = is_invocable_r<_Ret, _Fn, _Args...>::value;
template<typename _Ret, typename _Fn, typename... _Args>
  inline constexpr bool is_nothrow_invocable_r_v
    = is_nothrow_invocable_r<_Ret, _Fn, _Args...>::value;






  template<typename _Tp>
    struct has_unique_object_representations
    : bool_constant<__has_unique_object_representations(
      remove_cv_t<remove_all_extents_t<_Tp>>
      )>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };



  template<typename _Tp>
    inline constexpr bool has_unique_object_representations_v
      = has_unique_object_representations<_Tp>::value;






  template<typename _Tp>
    struct is_aggregate
    : bool_constant<__is_aggregate(remove_cv_t<_Tp>)>
    { };






  template<typename _Tp>
    inline constexpr bool is_aggregate_v = __is_aggregate(remove_cv_t<_Tp>);
# 3763 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp>
    struct remove_cvref
    { using type = __remove_cvref(_Tp); };
# 3780 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp>
    using remove_cvref_t = typename remove_cvref<_Tp>::type;
# 3790 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp>
    struct type_identity { using type = _Tp; };

  template<typename _Tp>
    using type_identity_t = typename type_identity<_Tp>::type;
# 3803 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp>
    struct unwrap_reference { using type = _Tp; };

  template<typename _Tp>
    struct unwrap_reference<reference_wrapper<_Tp>> { using type = _Tp&; };

  template<typename _Tp>
    using unwrap_reference_t = typename unwrap_reference<_Tp>::type;






  template<typename _Tp>
    struct unwrap_ref_decay { using type = unwrap_reference_t<decay_t<_Tp>>; };

  template<typename _Tp>
    using unwrap_ref_decay_t = typename unwrap_ref_decay<_Tp>::type;
# 3830 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp>
    inline constexpr bool is_bounded_array_v = __is_bounded_array(_Tp);
# 3844 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp>
    inline constexpr bool is_unbounded_array_v = __is_unbounded_array(_Tp);
# 3856 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp>
    struct is_bounded_array
    : public bool_constant<is_bounded_array_v<_Tp>>
    { };



  template<typename _Tp>
    struct is_unbounded_array
    : public bool_constant<is_unbounded_array_v<_Tp>>
    { };





  template<typename _Tp, typename _Up>
    struct is_layout_compatible
    : bool_constant<__is_layout_compatible(_Tp, _Up)>
    { };



  template<typename _Tp, typename _Up>
    constexpr bool is_layout_compatible_v
      = __is_layout_compatible(_Tp, _Up);







  template<typename _S1, typename _S2, typename _M1, typename _M2>
    constexpr bool
    is_corresponding_member(_M1 _S1::*__m1, _M2 _S2::*__m2) noexcept
    { return __builtin_is_corresponding_member(__m1, __m2); }







  template<typename _Base, typename _Derived>
    struct is_pointer_interconvertible_base_of
    : bool_constant<__is_pointer_interconvertible_base_of(_Base, _Derived)>
    { };



  template<typename _Base, typename _Derived>
    constexpr bool is_pointer_interconvertible_base_of_v
      = __is_pointer_interconvertible_base_of(_Base, _Derived);
# 3919 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp, typename _Mem>
    constexpr bool
    is_pointer_interconvertible_with_class(_Mem _Tp::*__mp) noexcept
    { return __builtin_is_pointer_interconvertible_with_class(__mp); }
# 4003 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  constexpr inline bool
  is_constant_evaluated() noexcept
  {



    return __builtin_is_constant_evaluated();

  }




  template<typename _From, typename _To>
    using __copy_cv = typename __match_cv_qualifiers<_From, _To>::__type;

  template<typename _Xp, typename _Yp>
    using __cond_res
      = decltype(false ? declval<_Xp(&)()>()() : declval<_Yp(&)()>()());

  template<typename _Ap, typename _Bp, typename = void>
    struct __common_ref_impl
    { };


  template<typename _Ap, typename _Bp>
    using __common_ref = typename __common_ref_impl<_Ap, _Bp>::type;


  template<typename _Xp, typename _Yp>
    using __condres_cvref
      = __cond_res<__copy_cv<_Xp, _Yp>&, __copy_cv<_Yp, _Xp>&>;


  template<typename _Xp, typename _Yp>
    struct __common_ref_impl<_Xp&, _Yp&, __void_t<__condres_cvref<_Xp, _Yp>>>
    : enable_if<is_reference_v<__condres_cvref<_Xp, _Yp>>,
  __condres_cvref<_Xp, _Yp>>
    { };


  template<typename _Xp, typename _Yp>
    using __common_ref_C = remove_reference_t<__common_ref<_Xp&, _Yp&>>&&;


  template<typename _Xp, typename _Yp>
    struct __common_ref_impl<_Xp&&, _Yp&&,
      _Require<is_convertible<_Xp&&, __common_ref_C<_Xp, _Yp>>,
        is_convertible<_Yp&&, __common_ref_C<_Xp, _Yp>>>>
    { using type = __common_ref_C<_Xp, _Yp>; };


  template<typename _Xp, typename _Yp>
    using __common_ref_D = __common_ref<const _Xp&, _Yp&>;


  template<typename _Xp, typename _Yp>
    struct __common_ref_impl<_Xp&&, _Yp&,
      _Require<is_convertible<_Xp&&, __common_ref_D<_Xp, _Yp>>>>
    { using type = __common_ref_D<_Xp, _Yp>; };


  template<typename _Xp, typename _Yp>
    struct __common_ref_impl<_Xp&, _Yp&&>
    : __common_ref_impl<_Yp&&, _Xp&>
    { };


  template<typename _Tp, typename _Up,
    template<typename> class _TQual, template<typename> class _UQual>
    struct basic_common_reference
    { };


  template<typename _Tp>
    struct __xref
    { template<typename _Up> using __type = __copy_cv<_Tp, _Up>; };

  template<typename _Tp>
    struct __xref<_Tp&>
    { template<typename _Up> using __type = __copy_cv<_Tp, _Up>&; };

  template<typename _Tp>
    struct __xref<_Tp&&>
    { template<typename _Up> using __type = __copy_cv<_Tp, _Up>&&; };

  template<typename _Tp1, typename _Tp2>
    using __basic_common_ref
      = typename basic_common_reference<remove_cvref_t<_Tp1>,
     remove_cvref_t<_Tp2>,
     __xref<_Tp1>::template __type,
     __xref<_Tp2>::template __type>::type;


  template<typename... _Tp>
    struct common_reference;

  template<typename... _Tp>
    using common_reference_t = typename common_reference<_Tp...>::type;


  template<>
    struct common_reference<>
    { };


  template<typename _Tp0>
    struct common_reference<_Tp0>
    { using type = _Tp0; };


  template<typename _Tp1, typename _Tp2, int _Bullet = 1, typename = void>
    struct __common_reference_impl
    : __common_reference_impl<_Tp1, _Tp2, _Bullet + 1>
    { };


  template<typename _Tp1, typename _Tp2>
    struct common_reference<_Tp1, _Tp2>
    : __common_reference_impl<_Tp1, _Tp2>
    { };


  template<typename _Tp1, typename _Tp2>
    struct __common_reference_impl<_Tp1&, _Tp2&, 1,
       void_t<__common_ref<_Tp1&, _Tp2&>>>
    { using type = __common_ref<_Tp1&, _Tp2&>; };

  template<typename _Tp1, typename _Tp2>
    struct __common_reference_impl<_Tp1&&, _Tp2&&, 1,
       void_t<__common_ref<_Tp1&&, _Tp2&&>>>
    { using type = __common_ref<_Tp1&&, _Tp2&&>; };

  template<typename _Tp1, typename _Tp2>
    struct __common_reference_impl<_Tp1&, _Tp2&&, 1,
       void_t<__common_ref<_Tp1&, _Tp2&&>>>
    { using type = __common_ref<_Tp1&, _Tp2&&>; };

  template<typename _Tp1, typename _Tp2>
    struct __common_reference_impl<_Tp1&&, _Tp2&, 1,
       void_t<__common_ref<_Tp1&&, _Tp2&>>>
    { using type = __common_ref<_Tp1&&, _Tp2&>; };


  template<typename _Tp1, typename _Tp2>
    struct __common_reference_impl<_Tp1, _Tp2, 2,
       void_t<__basic_common_ref<_Tp1, _Tp2>>>
    { using type = __basic_common_ref<_Tp1, _Tp2>; };


  template<typename _Tp1, typename _Tp2>
    struct __common_reference_impl<_Tp1, _Tp2, 3,
       void_t<__cond_res<_Tp1, _Tp2>>>
    { using type = __cond_res<_Tp1, _Tp2>; };


  template<typename _Tp1, typename _Tp2>
    struct __common_reference_impl<_Tp1, _Tp2, 4,
       void_t<common_type_t<_Tp1, _Tp2>>>
    { using type = common_type_t<_Tp1, _Tp2>; };


  template<typename _Tp1, typename _Tp2>
    struct __common_reference_impl<_Tp1, _Tp2, 5, void>
    { };


  template<typename _Tp1, typename _Tp2, typename... _Rest>
    struct common_reference<_Tp1, _Tp2, _Rest...>
    : __common_type_fold<common_reference<_Tp1, _Tp2>,
    __common_type_pack<_Rest...>>
    { };


  template<typename _Tp1, typename _Tp2, typename... _Rest>
    struct __common_type_fold<common_reference<_Tp1, _Tp2>,
         __common_type_pack<_Rest...>,
         void_t<common_reference_t<_Tp1, _Tp2>>>
    : public common_reference<common_reference_t<_Tp1, _Tp2>, _Rest...>
    { };







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
    inline constexpr _Tp*
    addressof(_Tp& __r) noexcept
    { return std::__addressof(__r); }



  template<typename _Tp>
    const _Tp* addressof(const _Tp&&) = delete;


  template <typename _Tp, typename _Up = _Tp>
    constexpr
    inline _Tp
    __exchange(_Tp& __obj, _Up&& __new_val)
    {
      _Tp __old_val = std::move(__obj);
      __obj = std::forward<_Up>(__new_val);
      return __old_val;
    }
# 217 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/move.h" 3
  template<typename _Tp>
    constexpr
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
    constexpr
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


  template<typename _Tp>
    inline constexpr size_t tuple_size_v = tuple_size<_Tp>::value;



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

      static_assert(is_integral_v<_Tp>);

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




  struct in_place_t {
    explicit in_place_t() = default;
  };

  inline constexpr in_place_t in_place{};

  template<typename _Tp> struct in_place_type_t
  {
    explicit in_place_type_t() = default;
  };

  template<typename _Tp>
    inline constexpr in_place_type_t<_Tp> in_place_type{};

  template<size_t _Idx> struct in_place_index_t
  {
    explicit in_place_index_t() = default;
  };

  template<size_t _Idx>
    inline constexpr in_place_index_t<_Idx> in_place_index{};

  template<typename>
    inline constexpr bool __is_in_place_type_v = false;

  template<typename _Tp>
    inline constexpr bool __is_in_place_type_v<in_place_type_t<_Tp>> = true;

  template<typename>
    inline constexpr bool __is_in_place_index_v = false;

  template<size_t _Nm>
    inline constexpr bool __is_in_place_index_v<in_place_index_t<_Nm>> = true;




  template<size_t _Np, typename... _Types>
    struct _Nth_type
    { using type = __type_pack_element<_Np, _Types...>; };
# 275 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/utility.h" 3
  namespace ranges::__detail
  {
    template<typename _Range>
      inline constexpr bool __is_subrange = false;
  }




  struct _Swallow_assign
  {
    template<class _Tp>
      constexpr const _Swallow_assign&
      operator=(const _Tp&) const noexcept
      { return *this; }
  };
# 309 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/utility.h" 3
  inline constexpr _Swallow_assign ignore{};
# 319 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/utility.h" 3

}
# 63 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 2 3


# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/compare" 1 3
# 38 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/compare" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/version.h" 1 3
# 39 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/compare" 2 3



# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/concepts" 1 3
# 38 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/concepts" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/version.h" 1 3
# 39 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/concepts" 2 3
# 50 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/concepts" 3
namespace std __attribute__ ((__visibility__ ("default")))
{




  namespace __detail
  {
    template<typename _Tp, typename _Up>
      concept __same_as = std::is_same_v<_Tp, _Up>;
  }


  template<typename _Tp, typename _Up>
    concept same_as
      = __detail::__same_as<_Tp, _Up> && __detail::__same_as<_Up, _Tp>;

  namespace __detail
  {
    template<typename _Tp, typename _Up>
      concept __different_from
 = !same_as<remove_cvref_t<_Tp>, remove_cvref_t<_Up>>;
  }


  template<typename _Derived, typename _Base>
    concept derived_from = __is_base_of(_Base, _Derived)
      && is_convertible_v<const volatile _Derived*, const volatile _Base*>;


  template<typename _From, typename _To>
    concept convertible_to = is_convertible_v<_From, _To>
      && requires { static_cast<_To>(std::declval<_From>()); };


  template<typename _Tp, typename _Up>
    concept common_reference_with
      = same_as<common_reference_t<_Tp, _Up>, common_reference_t<_Up, _Tp>>
      && convertible_to<_Tp, common_reference_t<_Tp, _Up>>
      && convertible_to<_Up, common_reference_t<_Tp, _Up>>;


  template<typename _Tp, typename _Up>
    concept common_with
      = same_as<common_type_t<_Tp, _Up>, common_type_t<_Up, _Tp>>
      && requires {
 static_cast<common_type_t<_Tp, _Up>>(std::declval<_Tp>());
 static_cast<common_type_t<_Tp, _Up>>(std::declval<_Up>());
      }
      && common_reference_with<add_lvalue_reference_t<const _Tp>,
          add_lvalue_reference_t<const _Up>>
      && common_reference_with<add_lvalue_reference_t<common_type_t<_Tp, _Up>>,
          common_reference_t<
     add_lvalue_reference_t<const _Tp>,
     add_lvalue_reference_t<const _Up>>>;



  template<typename _Tp>
    concept integral = is_integral_v<_Tp>;

  template<typename _Tp>
    concept signed_integral = integral<_Tp> && is_signed_v<_Tp>;

  template<typename _Tp>
    concept unsigned_integral = integral<_Tp> && !signed_integral<_Tp>;

  template<typename _Tp>
    concept floating_point = is_floating_point_v<_Tp>;

  namespace __detail
  {
    template<typename _Tp>
      using __cref = const remove_reference_t<_Tp>&;

    template<typename _Tp>
      concept __class_or_enum
 = is_class_v<_Tp> || is_union_v<_Tp> || is_enum_v<_Tp>;

    template<typename _Tp>
      constexpr bool __destructible_impl = false;
    template<typename _Tp>
      requires requires(_Tp& __t) { { __t.~_Tp() } noexcept; }
      constexpr bool __destructible_impl<_Tp> = true;

    template<typename _Tp>
      constexpr bool __destructible = __destructible_impl<_Tp>;
    template<typename _Tp>
      constexpr bool __destructible<_Tp&> = true;
    template<typename _Tp>
      constexpr bool __destructible<_Tp&&> = true;
    template<typename _Tp, size_t _Nm>
      constexpr bool __destructible<_Tp[_Nm]> = __destructible<_Tp>;

  }


  template<typename _Lhs, typename _Rhs>
    concept assignable_from
      = is_lvalue_reference_v<_Lhs>
      && common_reference_with<__detail::__cref<_Lhs>, __detail::__cref<_Rhs>>
      && requires(_Lhs __lhs, _Rhs&& __rhs) {
 { __lhs = static_cast<_Rhs&&>(__rhs) } -> same_as<_Lhs>;
      };


  template<typename _Tp>
    concept destructible = __detail::__destructible<_Tp>;


  template<typename _Tp, typename... _Args>
    concept constructible_from
      = destructible<_Tp> && is_constructible_v<_Tp, _Args...>;


  template<typename _Tp>
    concept default_initializable = constructible_from<_Tp>
      && requires
      {
 _Tp{};
 (void) ::new _Tp;
      };


  template<typename _Tp>
    concept move_constructible
    = constructible_from<_Tp, _Tp> && convertible_to<_Tp, _Tp>;


  template<typename _Tp>
    concept copy_constructible
      = move_constructible<_Tp>
      && constructible_from<_Tp, _Tp&> && convertible_to<_Tp&, _Tp>
      && constructible_from<_Tp, const _Tp&> && convertible_to<const _Tp&, _Tp>
      && constructible_from<_Tp, const _Tp> && convertible_to<const _Tp, _Tp>;



  namespace ranges
  {

    namespace __swap
    {
      template<typename _Tp> void swap(_Tp&, _Tp&) = delete;

      template<typename _Tp, typename _Up>
 concept __adl_swap
   = (std::__detail::__class_or_enum<remove_reference_t<_Tp>>
     || std::__detail::__class_or_enum<remove_reference_t<_Up>>)
   && requires(_Tp&& __t, _Up&& __u) {
     swap(static_cast<_Tp&&>(__t), static_cast<_Up&&>(__u));
   };

      struct _Swap
      {
      private:
 template<typename _Tp, typename _Up>
   static constexpr bool
   _S_noexcept()
   {
     if constexpr (__adl_swap<_Tp, _Up>)
       return noexcept(swap(std::declval<_Tp>(), std::declval<_Up>()));
     else
       return is_nothrow_move_constructible_v<remove_reference_t<_Tp>>
     && is_nothrow_move_assignable_v<remove_reference_t<_Tp>>;
   }

      public:
 template<typename _Tp, typename _Up>
   requires __adl_swap<_Tp, _Up>
   || (same_as<_Tp, _Up> && is_lvalue_reference_v<_Tp>
       && move_constructible<remove_reference_t<_Tp>>
       && assignable_from<_Tp, remove_reference_t<_Tp>>)
   constexpr void
   operator()(_Tp&& __t, _Up&& __u) const
   noexcept(_S_noexcept<_Tp, _Up>())
   {
     if constexpr (__adl_swap<_Tp, _Up>)
       swap(static_cast<_Tp&&>(__t), static_cast<_Up&&>(__u));
     else
       {
  auto __tmp = static_cast<remove_reference_t<_Tp>&&>(__t);
  __t = static_cast<remove_reference_t<_Tp>&&>(__u);
  __u = static_cast<remove_reference_t<_Tp>&&>(__tmp);
       }
   }

 template<typename _Tp, typename _Up, size_t _Num>
   requires requires(const _Swap& __swap, _Tp& __e1, _Up& __e2) {
     __swap(__e1, __e2);
   }
   constexpr void
   operator()(_Tp (&__e1)[_Num], _Up (&__e2)[_Num]) const
   noexcept(noexcept(std::declval<const _Swap&>()(*__e1, *__e2)))
   {
     for (size_t __n = 0; __n < _Num; ++__n)
       (*this)(__e1[__n], __e2[__n]);
   }
      };
    }


    inline namespace _Cpo {
      inline constexpr __swap::_Swap swap{};
    }
  }

  template<typename _Tp>
    concept swappable
      = requires(_Tp& __a, _Tp& __b) { ranges::swap(__a, __b); };

  template<typename _Tp, typename _Up>
    concept swappable_with = common_reference_with<_Tp, _Up>
      && requires(_Tp&& __t, _Up&& __u) {
 ranges::swap(static_cast<_Tp&&>(__t), static_cast<_Tp&&>(__t));
 ranges::swap(static_cast<_Up&&>(__u), static_cast<_Up&&>(__u));
 ranges::swap(static_cast<_Tp&&>(__t), static_cast<_Up&&>(__u));
 ranges::swap(static_cast<_Up&&>(__u), static_cast<_Tp&&>(__t));
      };



  template<typename _Tp>
    concept movable = is_object_v<_Tp> && move_constructible<_Tp>
      && assignable_from<_Tp&, _Tp> && swappable<_Tp>;

  template<typename _Tp>
    concept copyable = copy_constructible<_Tp> && movable<_Tp>
      && assignable_from<_Tp&, _Tp&> && assignable_from<_Tp&, const _Tp&>
      && assignable_from<_Tp&, const _Tp>;

  template<typename _Tp>
    concept semiregular = copyable<_Tp> && default_initializable<_Tp>;




  namespace __detail
  {
    template<typename _Tp>
      concept __boolean_testable_impl = convertible_to<_Tp, bool>;

    template<typename _Tp>
      concept __boolean_testable
 = __boolean_testable_impl<_Tp>
   && requires(_Tp&& __t)
   { { !static_cast<_Tp&&>(__t) } -> __boolean_testable_impl; };
  }



  namespace __detail
  {
    template<typename _Tp, typename _Up>
      concept __weakly_eq_cmp_with
 = requires(__detail::__cref<_Tp> __t, __detail::__cref<_Up> __u) {
   { __t == __u } -> __boolean_testable;
   { __t != __u } -> __boolean_testable;
   { __u == __t } -> __boolean_testable;
   { __u != __t } -> __boolean_testable;
 };
  }

  template<typename _Tp>
    concept equality_comparable = __detail::__weakly_eq_cmp_with<_Tp, _Tp>;

  template<typename _Tp, typename _Up>
    concept equality_comparable_with
      = equality_comparable<_Tp> && equality_comparable<_Up>
      && common_reference_with<__detail::__cref<_Tp>, __detail::__cref<_Up>>
      && equality_comparable<common_reference_t<__detail::__cref<_Tp>,
      __detail::__cref<_Up>>>
      && __detail::__weakly_eq_cmp_with<_Tp, _Up>;

  namespace __detail
  {
    template<typename _Tp, typename _Up>
      concept __partially_ordered_with
 = requires(const remove_reference_t<_Tp>& __t,
     const remove_reference_t<_Up>& __u) {
   { __t < __u } -> __boolean_testable;
   { __t > __u } -> __boolean_testable;
   { __t <= __u } -> __boolean_testable;
   { __t >= __u } -> __boolean_testable;
   { __u < __t } -> __boolean_testable;
   { __u > __t } -> __boolean_testable;
   { __u <= __t } -> __boolean_testable;
   { __u >= __t } -> __boolean_testable;
 };
  }


  template<typename _Tp>
    concept totally_ordered
      = equality_comparable<_Tp>
      && __detail::__partially_ordered_with<_Tp, _Tp>;

  template<typename _Tp, typename _Up>
    concept totally_ordered_with
      = totally_ordered<_Tp> && totally_ordered<_Up>
      && equality_comparable_with<_Tp, _Up>
      && totally_ordered<common_reference_t<__detail::__cref<_Tp>,
         __detail::__cref<_Up>>>
      && __detail::__partially_ordered_with<_Tp, _Up>;

  template<typename _Tp>
    concept regular = semiregular<_Tp> && equality_comparable<_Tp>;




  template<typename _Fn, typename... _Args>
    concept invocable = is_invocable_v<_Fn, _Args...>;


  template<typename _Fn, typename... _Args>
    concept regular_invocable = invocable<_Fn, _Args...>;


  template<typename _Fn, typename... _Args>
    concept predicate = regular_invocable<_Fn, _Args...>
      && __detail::__boolean_testable<invoke_result_t<_Fn, _Args...>>;


  template<typename _Rel, typename _Tp, typename _Up>
    concept relation
      = predicate<_Rel, _Tp, _Tp> && predicate<_Rel, _Up, _Up>
      && predicate<_Rel, _Tp, _Up> && predicate<_Rel, _Up, _Tp>;


  template<typename _Rel, typename _Tp, typename _Up>
    concept equivalence_relation = relation<_Rel, _Tp, _Up>;


  template<typename _Rel, typename _Tp, typename _Up>
    concept strict_weak_order = relation<_Rel, _Tp, _Up>;


}
# 43 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/compare" 2 3

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wpedantic"
#pragma GCC diagnostic ignored "-Wzero-as-null-pointer-constant"

namespace std __attribute__ ((__visibility__ ("default")))
{


  namespace __cmp_cat
  {
    using type = signed char;

    enum class _Ord : type { equivalent = 0, less = -1, greater = 1 };

    enum class _Ncmp : type { _Unordered = 2 };

    struct __unspec
    {
      consteval __unspec(__unspec*) noexcept { }
    };
  }

  class partial_ordering
  {

    __cmp_cat::type _M_value;

    constexpr explicit
    partial_ordering(__cmp_cat::_Ord __v) noexcept
    : _M_value(__cmp_cat::type(__v))
    { }

    constexpr explicit
    partial_ordering(__cmp_cat::_Ncmp __v) noexcept
    : _M_value(__cmp_cat::type(__v))
    { }

    friend class weak_ordering;
    friend class strong_ordering;

  public:

    static const partial_ordering less;
    static const partial_ordering equivalent;
    static const partial_ordering greater;
    static const partial_ordering unordered;


    [[nodiscard]]
    friend constexpr bool
    operator==(partial_ordering __v, __cmp_cat::__unspec) noexcept
    { return __v._M_value == 0; }

    [[nodiscard]]
    friend constexpr bool
    operator==(partial_ordering, partial_ordering) noexcept = default;

    [[nodiscard]]
    friend constexpr bool
    operator< (partial_ordering __v, __cmp_cat::__unspec) noexcept
    { return __v._M_value == -1; }

    [[nodiscard]]
    friend constexpr bool
    operator> (partial_ordering __v, __cmp_cat::__unspec) noexcept
    { return __v._M_value == 1; }

    [[nodiscard]]
    friend constexpr bool
    operator<=(partial_ordering __v, __cmp_cat::__unspec) noexcept
    { return __v._M_value <= 0; }

    [[nodiscard]]
    friend constexpr bool
    operator>=(partial_ordering __v, __cmp_cat::__unspec) noexcept
    { return __cmp_cat::type(__v._M_value & 1) == __v._M_value; }

    [[nodiscard]]
    friend constexpr bool
    operator< (__cmp_cat::__unspec, partial_ordering __v) noexcept
    { return __v._M_value == 1; }

    [[nodiscard]]
    friend constexpr bool
    operator> (__cmp_cat::__unspec, partial_ordering __v) noexcept
    { return __v._M_value == -1; }

    [[nodiscard]]
    friend constexpr bool
    operator<=(__cmp_cat::__unspec, partial_ordering __v) noexcept
    { return __cmp_cat::type(__v._M_value & 1) == __v._M_value; }

    [[nodiscard]]
    friend constexpr bool
    operator>=(__cmp_cat::__unspec, partial_ordering __v) noexcept
    { return 0 >= __v._M_value; }

    [[nodiscard]]
    friend constexpr partial_ordering
    operator<=>(partial_ordering __v, __cmp_cat::__unspec) noexcept
    { return __v; }

    [[nodiscard]]
    friend constexpr partial_ordering
    operator<=>(__cmp_cat::__unspec, partial_ordering __v) noexcept
    {
      if (__v._M_value & 1)
 return partial_ordering(__cmp_cat::_Ord(-__v._M_value));
      else
 return __v;
    }
  };


  inline constexpr partial_ordering
  partial_ordering::less(__cmp_cat::_Ord::less);

  inline constexpr partial_ordering
  partial_ordering::equivalent(__cmp_cat::_Ord::equivalent);

  inline constexpr partial_ordering
  partial_ordering::greater(__cmp_cat::_Ord::greater);

  inline constexpr partial_ordering
  partial_ordering::unordered(__cmp_cat::_Ncmp::_Unordered);

  class weak_ordering
  {
    __cmp_cat::type _M_value;

    constexpr explicit
    weak_ordering(__cmp_cat::_Ord __v) noexcept : _M_value(__cmp_cat::type(__v))
    { }

    friend class strong_ordering;

  public:

    static const weak_ordering less;
    static const weak_ordering equivalent;
    static const weak_ordering greater;

    [[nodiscard]]
    constexpr operator partial_ordering() const noexcept
    { return partial_ordering(__cmp_cat::_Ord(_M_value)); }


    [[nodiscard]]
    friend constexpr bool
    operator==(weak_ordering __v, __cmp_cat::__unspec) noexcept
    { return __v._M_value == 0; }

    [[nodiscard]]
    friend constexpr bool
    operator==(weak_ordering, weak_ordering) noexcept = default;

    [[nodiscard]]
    friend constexpr bool
    operator< (weak_ordering __v, __cmp_cat::__unspec) noexcept
    { return __v._M_value < 0; }

    [[nodiscard]]
    friend constexpr bool
    operator> (weak_ordering __v, __cmp_cat::__unspec) noexcept
    { return __v._M_value > 0; }

    [[nodiscard]]
    friend constexpr bool
    operator<=(weak_ordering __v, __cmp_cat::__unspec) noexcept
    { return __v._M_value <= 0; }

    [[nodiscard]]
    friend constexpr bool
    operator>=(weak_ordering __v, __cmp_cat::__unspec) noexcept
    { return __v._M_value >= 0; }

    [[nodiscard]]
    friend constexpr bool
    operator< (__cmp_cat::__unspec, weak_ordering __v) noexcept
    { return 0 < __v._M_value; }

    [[nodiscard]]
    friend constexpr bool
    operator> (__cmp_cat::__unspec, weak_ordering __v) noexcept
    { return 0 > __v._M_value; }

    [[nodiscard]]
    friend constexpr bool
    operator<=(__cmp_cat::__unspec, weak_ordering __v) noexcept
    { return 0 <= __v._M_value; }

    [[nodiscard]]
    friend constexpr bool
    operator>=(__cmp_cat::__unspec, weak_ordering __v) noexcept
    { return 0 >= __v._M_value; }

    [[nodiscard]]
    friend constexpr weak_ordering
    operator<=>(weak_ordering __v, __cmp_cat::__unspec) noexcept
    { return __v; }

    [[nodiscard]]
    friend constexpr weak_ordering
    operator<=>(__cmp_cat::__unspec, weak_ordering __v) noexcept
    { return weak_ordering(__cmp_cat::_Ord(-__v._M_value)); }
  };


  inline constexpr weak_ordering
  weak_ordering::less(__cmp_cat::_Ord::less);

  inline constexpr weak_ordering
  weak_ordering::equivalent(__cmp_cat::_Ord::equivalent);

  inline constexpr weak_ordering
  weak_ordering::greater(__cmp_cat::_Ord::greater);

  class strong_ordering
  {
    __cmp_cat::type _M_value;

    constexpr explicit
    strong_ordering(__cmp_cat::_Ord __v) noexcept
    : _M_value(__cmp_cat::type(__v))
    { }

  public:

    static const strong_ordering less;
    static const strong_ordering equal;
    static const strong_ordering equivalent;
    static const strong_ordering greater;

    [[nodiscard]]
    constexpr operator partial_ordering() const noexcept
    { return partial_ordering(__cmp_cat::_Ord(_M_value)); }

    [[nodiscard]]
    constexpr operator weak_ordering() const noexcept
    { return weak_ordering(__cmp_cat::_Ord(_M_value)); }


    [[nodiscard]]
    friend constexpr bool
    operator==(strong_ordering __v, __cmp_cat::__unspec) noexcept
    { return __v._M_value == 0; }

    [[nodiscard]]
    friend constexpr bool
    operator==(strong_ordering, strong_ordering) noexcept = default;

    [[nodiscard]]
    friend constexpr bool
    operator< (strong_ordering __v, __cmp_cat::__unspec) noexcept
    { return __v._M_value < 0; }

    [[nodiscard]]
    friend constexpr bool
    operator> (strong_ordering __v, __cmp_cat::__unspec) noexcept
    { return __v._M_value > 0; }

    [[nodiscard]]
    friend constexpr bool
    operator<=(strong_ordering __v, __cmp_cat::__unspec) noexcept
    { return __v._M_value <= 0; }

    [[nodiscard]]
    friend constexpr bool
    operator>=(strong_ordering __v, __cmp_cat::__unspec) noexcept
    { return __v._M_value >= 0; }

    [[nodiscard]]
    friend constexpr bool
    operator< (__cmp_cat::__unspec, strong_ordering __v) noexcept
    { return 0 < __v._M_value; }

    [[nodiscard]]
    friend constexpr bool
    operator> (__cmp_cat::__unspec, strong_ordering __v) noexcept
    { return 0 > __v._M_value; }

    [[nodiscard]]
    friend constexpr bool
    operator<=(__cmp_cat::__unspec, strong_ordering __v) noexcept
    { return 0 <= __v._M_value; }

    [[nodiscard]]
    friend constexpr bool
    operator>=(__cmp_cat::__unspec, strong_ordering __v) noexcept
    { return 0 >= __v._M_value; }

    [[nodiscard]]
    friend constexpr strong_ordering
    operator<=>(strong_ordering __v, __cmp_cat::__unspec) noexcept
    { return __v; }

    [[nodiscard]]
    friend constexpr strong_ordering
    operator<=>(__cmp_cat::__unspec, strong_ordering __v) noexcept
    { return strong_ordering(__cmp_cat::_Ord(-__v._M_value)); }
  };


  inline constexpr strong_ordering
  strong_ordering::less(__cmp_cat::_Ord::less);

  inline constexpr strong_ordering
  strong_ordering::equal(__cmp_cat::_Ord::equivalent);

  inline constexpr strong_ordering
  strong_ordering::equivalent(__cmp_cat::_Ord::equivalent);

  inline constexpr strong_ordering
  strong_ordering::greater(__cmp_cat::_Ord::greater);



  [[nodiscard]]
  constexpr bool
  is_eq(partial_ordering __cmp) noexcept
  { return __cmp == 0; }

  [[nodiscard]]
  constexpr bool
  is_neq(partial_ordering __cmp) noexcept
  { return __cmp != 0; }

  [[nodiscard]]
  constexpr bool
  is_lt (partial_ordering __cmp) noexcept
  { return __cmp < 0; }

  [[nodiscard]]
  constexpr bool
  is_lteq(partial_ordering __cmp) noexcept
  { return __cmp <= 0; }

  [[nodiscard]]
  constexpr bool
  is_gt (partial_ordering __cmp) noexcept
  { return __cmp > 0; }

  [[nodiscard]]
  constexpr bool
  is_gteq(partial_ordering __cmp) noexcept
  { return __cmp >= 0; }

  namespace __detail
  {
    template<typename _Tp>
      inline constexpr unsigned __cmp_cat_id = 1;
    template<>
      inline constexpr unsigned __cmp_cat_id<partial_ordering> = 2;
    template<>
      inline constexpr unsigned __cmp_cat_id<weak_ordering> = 4;
    template<>
      inline constexpr unsigned __cmp_cat_id<strong_ordering> = 8;

    template<typename... _Ts>
      constexpr auto __common_cmp_cat()
      {
 constexpr unsigned __cats = (__cmp_cat_id<_Ts> | ...);

 if constexpr (__cats & 1)
   return;


 else if constexpr (bool(__cats & __cmp_cat_id<partial_ordering>))
   return partial_ordering::equivalent;


 else if constexpr (bool(__cats & __cmp_cat_id<weak_ordering>))
   return weak_ordering::equivalent;

 else
   return strong_ordering::equivalent;
      }
  }


  template<typename... _Ts>
    struct common_comparison_category
    {
      using type = decltype(__detail::__common_cmp_cat<_Ts...>());
    };



  template<typename _Tp>
    struct common_comparison_category<_Tp>
    { using type = void; };

  template<>
    struct common_comparison_category<partial_ordering>
    { using type = partial_ordering; };

  template<>
    struct common_comparison_category<weak_ordering>
    { using type = weak_ordering; };

  template<>
    struct common_comparison_category<strong_ordering>
    { using type = strong_ordering; };

  template<>
    struct common_comparison_category<>
    { using type = strong_ordering; };

  template<typename... _Ts>
    using common_comparison_category_t
      = typename common_comparison_category<_Ts...>::type;



  namespace __detail
  {
    template<typename _Tp, typename _Cat>
      concept __compares_as
 = same_as<common_comparison_category_t<_Tp, _Cat>, _Cat>;
  }


  template<typename _Tp, typename _Cat = partial_ordering>
    concept three_way_comparable
      = __detail::__weakly_eq_cmp_with<_Tp, _Tp>
      && __detail::__partially_ordered_with<_Tp, _Tp>
      && requires(const remove_reference_t<_Tp>& __a,
    const remove_reference_t<_Tp>& __b)
      {
 { __a <=> __b } -> __detail::__compares_as<_Cat>;
      };

  template<typename _Tp, typename _Up, typename _Cat = partial_ordering>
    concept three_way_comparable_with
      = three_way_comparable<_Tp, _Cat>
      && three_way_comparable<_Up, _Cat>
      && common_reference_with<const remove_reference_t<_Tp>&,
          const remove_reference_t<_Up>&>
      && three_way_comparable<
   common_reference_t<const remove_reference_t<_Tp>&,
        const remove_reference_t<_Up>&>, _Cat>
      && __detail::__weakly_eq_cmp_with<_Tp, _Up>
      && __detail::__partially_ordered_with<_Tp, _Up>
      && requires(const remove_reference_t<_Tp>& __t,
    const remove_reference_t<_Up>& __u)
      {
 { __t <=> __u } -> __detail::__compares_as<_Cat>;
 { __u <=> __t } -> __detail::__compares_as<_Cat>;
      };

  namespace __detail
  {
    template<typename _Tp, typename _Up>
      using __cmp3way_res_t
 = decltype(std::declval<_Tp>() <=> std::declval<_Up>());






    template<typename _Tp, typename _Up>
      struct __cmp3way_res_impl
      { };

    template<typename _Tp, typename _Up>
      requires requires { typename __cmp3way_res_t<__cref<_Tp>, __cref<_Up>>; }
      struct __cmp3way_res_impl<_Tp, _Up>
      {
 using type = __cmp3way_res_t<__cref<_Tp>, __cref<_Up>>;
      };
  }


  template<typename _Tp, typename _Up = _Tp>
    struct compare_three_way_result
    : __detail::__cmp3way_res_impl<_Tp, _Up>
    { };


  template<typename _Tp, typename _Up = _Tp>
    using compare_three_way_result_t
      = typename __detail::__cmp3way_res_impl<_Tp, _Up>::type;

  namespace __detail
  {




    template<typename _Tp, typename _Up>
      concept __3way_builtin_ptr_cmp
 = requires(_Tp&& __t, _Up&& __u)
   { static_cast<_Tp&&>(__t) <=> static_cast<_Up&&>(__u); }
   && convertible_to<_Tp, const volatile void*>
   && convertible_to<_Up, const volatile void*>
   && ! requires(_Tp&& __t, _Up&& __u)
   { operator<=>(static_cast<_Tp&&>(__t), static_cast<_Up&&>(__u)); }
   && ! requires(_Tp&& __t, _Up&& __u)
   { static_cast<_Tp&&>(__t).operator<=>(static_cast<_Up&&>(__u)); };
  }





  struct compare_three_way
  {
    template<typename _Tp, typename _Up>
      requires three_way_comparable_with<_Tp, _Up>
      constexpr auto
      operator() [[nodiscard]] (_Tp&& __t, _Up&& __u) const
      noexcept(noexcept(std::declval<_Tp>() <=> std::declval<_Up>()))
      {
 if constexpr (__detail::__3way_builtin_ptr_cmp<_Tp, _Up>)
   {
     auto __pt = static_cast<const volatile void*>(__t);
     auto __pu = static_cast<const volatile void*>(__u);
     if (std::__is_constant_evaluated())
       return __pt <=> __pu;
     auto __it = reinterpret_cast<long unsigned int>(__pt);
     auto __iu = reinterpret_cast<long unsigned int>(__pu);
     return __it <=> __iu;
   }
 else
   return static_cast<_Tp&&>(__t) <=> static_cast<_Up&&>(__u);
      }

    using is_transparent = void;
  };



  namespace __compare
  {
    template<floating_point _Tp>
      constexpr weak_ordering
      __fp_weak_ordering(_Tp __e, _Tp __f)
      {


 auto __cat = [](_Tp __fp) -> int {
   const int __sign = __builtin_signbit(__fp) ? -1 : 1;
   if (__builtin_isnormal(__fp))
     return (__fp == 0 ? 1 : 3) * __sign;
   if (__builtin_isnan(__fp))
     return 5 * __sign;
   if (int __inf = __builtin_isinf_sign(__fp))
     return 4 * __inf;
   return 2 * __sign;
 };

 auto __po = __e <=> __f;
 if (is_lt(__po))
   return weak_ordering::less;
 else if (is_gt(__po))
   return weak_ordering::greater;
 else if (__po == partial_ordering::equivalent)
   return weak_ordering::equivalent;
 else
   {

     auto __isnan_sign = [](_Tp __fp) -> int {
       return __builtin_isnan(__fp)
  ? __builtin_signbit(__fp) ? -1 : 1
  : 0;
     };
     auto __ord = __isnan_sign(__e) <=> __isnan_sign(__f);
     if (is_eq(__ord))
       return weak_ordering::equivalent;
     else if (is_lt(__ord))
       return weak_ordering::less;
     else
       return weak_ordering::greater;
   }
      }

    void strong_order() = delete;

    template<typename _Tp, typename _Up>
      concept __adl_strong = requires(_Tp&& __t, _Up&& __u)
 {
   strong_ordering(strong_order(static_cast<_Tp&&>(__t),
           static_cast<_Up&&>(__u)));
 };

    void weak_order() = delete;

    template<typename _Tp, typename _Up>
      concept __adl_weak = requires(_Tp&& __t, _Up&& __u)
 {
   weak_ordering(weak_order(static_cast<_Tp&&>(__t),
       static_cast<_Up&&>(__u)));
 };

    void partial_order() = delete;

    template<typename _Tp, typename _Up>
      concept __adl_partial = requires(_Tp&& __t, _Up&& __u)
 {
   partial_ordering(partial_order(static_cast<_Tp&&>(__t),
      static_cast<_Up&&>(__u)));
 };

    template<typename _Ord, typename _Tp, typename _Up>
      concept __cmp3way = requires(_Tp&& __t, _Up&& __u, compare_three_way __c)
 {
   _Ord(__c(static_cast<_Tp&&>(__t), static_cast<_Up&&>(__u)));
 };

    template<typename _Tp, typename _Up>
      concept __strongly_ordered
 = __adl_strong<_Tp, _Up>
   || floating_point<remove_reference_t<_Tp>>
   || __cmp3way<strong_ordering, _Tp, _Up>;

    template<typename _Tp, typename _Up>
      concept __decayed_same_as = same_as<decay_t<_Tp>, decay_t<_Up>>;

    class _Strong_order
    {
      template<typename _Tp, typename _Up>
 static constexpr bool
 _S_noexcept()
 {
   if constexpr (floating_point<decay_t<_Tp>>)
     return true;
   else if constexpr (__adl_strong<_Tp, _Up>)
     return noexcept(strong_ordering(strong_order(std::declval<_Tp>(),
        std::declval<_Up>())));
   else if constexpr (__cmp3way<strong_ordering, _Tp, _Up>)
     return noexcept(compare_three_way()(std::declval<_Tp>(),
      std::declval<_Up>()));
 }

      friend class _Weak_order;
      friend class _Strong_fallback;


      enum class _Fp_fmt
      {
 _Binary16, _Binary32, _Binary64, _Binary128,
 _X86_80bit,
 _M68k_80bit,
 _Dbldbl,
 _Bfloat16,
      };
# 705 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/compare" 3
      template<typename _Tp>
 static consteval _Fp_fmt
 _S_fp_fmt() noexcept
 {

   using enum _Fp_fmt;
# 725 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/compare" 3
   if constexpr (__is_same(_Tp, long double))
     return (-16381) == -16381 ? _X86_80bit : _M68k_80bit;


   if constexpr (__is_same(_Tp, __float80))
     return _X86_80bit;






   constexpr int __width = sizeof(_Tp) * 8;

   if constexpr (__width == 16)
     return _Binary16;
   else if constexpr (__width == 32)
     return _Binary32;
   else if constexpr (__width == 64)
     return _Binary64;
   else if constexpr (__width == 128)
     return _Binary128;
 }


      using int64_t = long int;
      using int32_t = int;
      using int16_t = short int;
      using uint64_t = long unsigned int;
      using uint16_t = short unsigned int;


      template<typename _Tp>
 struct _Int
 {

   uint64_t _M_lo;
   _Tp _M_hi;





   constexpr explicit
   _Int(_Tp __hi, uint64_t __lo) noexcept : _M_hi(__hi)
   { _M_lo = __lo; }

   constexpr explicit
   _Int(uint64_t __lo) noexcept : _M_hi(0)
   { _M_lo = __lo; }

   constexpr bool operator==(const _Int&) const = default;
# 787 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/compare" 3
   constexpr _Int&
   operator^=(const _Int& __rhs) noexcept
   {
     _M_hi ^= __rhs._M_hi;
     _M_lo ^= __rhs._M_lo;
     return *this;
   }

   constexpr strong_ordering
   operator<=>(const _Int& __rhs) const noexcept
   {
     strong_ordering __cmp = _M_hi <=> __rhs._M_hi;
     if (__cmp != strong_ordering::equal)
       return __cmp;
     return _M_lo <=> __rhs._M_lo;
   }
 };

      template<typename _Tp>
 static constexpr _Tp
 _S_compl(_Tp __t) noexcept
 {
   constexpr int __width = sizeof(_Tp) * 8;

   make_unsigned_t<_Tp> __sign = __t >> (__width - 1);


   return __t ^ (__sign >> 1);
 }


      template<typename _Tp>
 static constexpr _Int<_Tp>
 _S_compl(_Int<_Tp> __t) noexcept
 {
   constexpr int __width = sizeof(_Tp) * 8;
   make_unsigned_t<_Tp> __sign = __t._M_hi >> (__width - 1);
   __t._M_hi ^= (__sign >> 1 );
   uint64_t __sign64 = (_Tp)__sign;
   __t._M_lo ^= __sign64;
   return __t;
 }


      template<typename _Tp>
 constexpr static auto
 _S_fp_bits(_Tp __val) noexcept
 {
   if constexpr (sizeof(_Tp) == sizeof(int64_t))
     return __builtin_bit_cast(int64_t, __val);
   else if constexpr (sizeof(_Tp) == sizeof(int32_t))
     return __builtin_bit_cast(int32_t, __val);
   else if constexpr (sizeof(_Tp) == sizeof(int16_t))
     return __builtin_bit_cast(int16_t, __val);
   else
     {

       using enum _Fp_fmt;

       constexpr auto __fmt = _S_fp_fmt<_Tp>();
       if constexpr (__fmt == _X86_80bit)
  {
    if constexpr (sizeof(_Tp) == 3 * sizeof(int32_t))
      {
        auto __ival = __builtin_bit_cast(_Int<int32_t>, __val);
        return _Int<int16_t>(__ival._M_hi, __ival._M_lo);
      }
    else
      {
        auto __ival = __builtin_bit_cast(_Int<int64_t>, __val);
        return _Int<int16_t>(__ival._M_hi, __ival._M_lo);
      }
  }
       else if constexpr (__fmt == _M68k_80bit)
  {
    auto __ival = __builtin_bit_cast(_Int<int32_t>, __val);
    return _Int<int16_t>(__ival._M_hi >> 16, __ival._M_lo);
  }
       else if constexpr (sizeof(_Tp) == 2 * sizeof(int64_t))
  {

    return __builtin_bit_cast(__int128, __val);



  }
       else
  static_assert(sizeof(_Tp) == sizeof(int64_t),
         "unsupported floating-point type");
     }
 }

      template<typename _Tp>
 static constexpr strong_ordering
 _S_fp_cmp(_Tp __x, _Tp __y) noexcept
 {
# 896 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/compare" 3
   auto __ix = _S_fp_bits(__x);
   auto __iy = _S_fp_bits(__y);

   if (__ix == __iy)
     return strong_ordering::equal;


   using enum _Fp_fmt;

   constexpr auto __fmt = _S_fp_fmt<_Tp>();

   if constexpr (__fmt == _Dbldbl)
     {


       struct _Unpacked { double _M_hi; int64_t _M_lo; };
       auto __x2 = __builtin_bit_cast(_Unpacked, __x);
       auto __y2 = __builtin_bit_cast(_Unpacked, __y);


       auto __cmp = _S_fp_cmp(__x2._M_hi, __y2._M_hi);
       if (__cmp != strong_ordering::equal)
  return __cmp;



       if (__builtin_isnan(__x2._M_hi))
  return strong_ordering::equal;


       if (((__x2._M_lo | __y2._M_lo) & 0x7fffffffffffffffULL) == 0)
  return strong_ordering::equal;


       return _S_compl(__x2._M_lo) <=> _S_compl(__y2._M_lo);
     }
   else
     {
       if constexpr (__fmt == _M68k_80bit)
  {



    constexpr uint16_t __maxexp = 0x7fff;
    if ((__ix._M_hi & __maxexp) == __maxexp)
      __ix._M_lo |= 1ull << 63;
    if ((__iy._M_hi & __maxexp) == __maxexp)
      __iy._M_lo |= 1ull << 63;
  }
       else
  {
# 963 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/compare" 3
  }
       return _S_compl(__ix) <=> _S_compl(__iy);
     }
 }

    public:
      template<typename _Tp, __decayed_same_as<_Tp> _Up>
 requires __strongly_ordered<_Tp, _Up>
 constexpr strong_ordering
 operator() [[nodiscard]] (_Tp&& __e, _Up&& __f) const
 noexcept(_S_noexcept<_Tp, _Up>())
 {
   if constexpr (floating_point<decay_t<_Tp>>)
     return _S_fp_cmp(__e, __f);
   else if constexpr (__adl_strong<_Tp, _Up>)
     return strong_ordering(strong_order(static_cast<_Tp&&>(__e),
      static_cast<_Up&&>(__f)));
   else if constexpr (__cmp3way<strong_ordering, _Tp, _Up>)
     return compare_three_way()(static_cast<_Tp&&>(__e),
           static_cast<_Up&&>(__f));
 }
    };

    template<typename _Tp, typename _Up>
      concept __weakly_ordered
 = floating_point<remove_reference_t<_Tp>>
   || __adl_weak<_Tp, _Up>
   || __cmp3way<weak_ordering, _Tp, _Up>
   || __strongly_ordered<_Tp, _Up>;

    class _Weak_order
    {
      template<typename _Tp, typename _Up>
 static constexpr bool
 _S_noexcept()
 {
   if constexpr (floating_point<decay_t<_Tp>>)
     return true;
   else if constexpr (__adl_weak<_Tp, _Up>)
     return noexcept(weak_ordering(weak_order(std::declval<_Tp>(),
           std::declval<_Up>())));
   else if constexpr (__cmp3way<weak_ordering, _Tp, _Up>)
     return noexcept(compare_three_way()(std::declval<_Tp>(),
      std::declval<_Up>()));
   else if constexpr (__strongly_ordered<_Tp, _Up>)
     return _Strong_order::_S_noexcept<_Tp, _Up>();
 }

      friend class _Partial_order;
      friend class _Weak_fallback;

    public:
      template<typename _Tp, __decayed_same_as<_Tp> _Up>
 requires __weakly_ordered<_Tp, _Up>
 constexpr weak_ordering
 operator() [[nodiscard]] (_Tp&& __e, _Up&& __f) const
 noexcept(_S_noexcept<_Tp, _Up>())
 {
   if constexpr (floating_point<decay_t<_Tp>>)
     return __compare::__fp_weak_ordering(__e, __f);
   else if constexpr (__adl_weak<_Tp, _Up>)
     return weak_ordering(weak_order(static_cast<_Tp&&>(__e),
         static_cast<_Up&&>(__f)));
   else if constexpr (__cmp3way<weak_ordering, _Tp, _Up>)
     return compare_three_way()(static_cast<_Tp&&>(__e),
           static_cast<_Up&&>(__f));
   else if constexpr (__strongly_ordered<_Tp, _Up>)
     return _Strong_order{}(static_cast<_Tp&&>(__e),
       static_cast<_Up&&>(__f));
 }
    };

    template<typename _Tp, typename _Up>
      concept __partially_ordered
 = __adl_partial<_Tp, _Up>
 || __cmp3way<partial_ordering, _Tp, _Up>
 || __weakly_ordered<_Tp, _Up>;

    class _Partial_order
    {
      template<typename _Tp, typename _Up>
 static constexpr bool
 _S_noexcept()
 {
   if constexpr (__adl_partial<_Tp, _Up>)
     return noexcept(partial_ordering(partial_order(std::declval<_Tp>(),
        std::declval<_Up>())));
   else if constexpr (__cmp3way<partial_ordering, _Tp, _Up>)
     return noexcept(compare_three_way()(std::declval<_Tp>(),
      std::declval<_Up>()));
   else if constexpr (__weakly_ordered<_Tp, _Up>)
     return _Weak_order::_S_noexcept<_Tp, _Up>();
 }

      friend class _Partial_fallback;

    public:
      template<typename _Tp, __decayed_same_as<_Tp> _Up>
 requires __partially_ordered<_Tp, _Up>
 constexpr partial_ordering
 operator() [[nodiscard]] (_Tp&& __e, _Up&& __f) const
 noexcept(_S_noexcept<_Tp, _Up>())
 {
   if constexpr (__adl_partial<_Tp, _Up>)
     return partial_ordering(partial_order(static_cast<_Tp&&>(__e),
        static_cast<_Up&&>(__f)));
   else if constexpr (__cmp3way<partial_ordering, _Tp, _Up>)
     return compare_three_way()(static_cast<_Tp&&>(__e),
           static_cast<_Up&&>(__f));
   else if constexpr (__weakly_ordered<_Tp, _Up>)
     return _Weak_order{}(static_cast<_Tp&&>(__e),
     static_cast<_Up&&>(__f));
 }
    };

    template<typename _Tp, typename _Up>
      concept __op_eq_lt = requires(_Tp&& __t, _Up&& __u)
 {
   { static_cast<_Tp&&>(__t) == static_cast<_Up&&>(__u) }
     -> convertible_to<bool>;
   { static_cast<_Tp&&>(__t) < static_cast<_Up&&>(__u) }
     -> convertible_to<bool>;
 };

    class _Strong_fallback
    {
      template<typename _Tp, typename _Up>
 static constexpr bool
 _S_noexcept()
 {
   if constexpr (__strongly_ordered<_Tp, _Up>)
     return _Strong_order::_S_noexcept<_Tp, _Up>();
   else
     return noexcept(bool(std::declval<_Tp>() == std::declval<_Up>()))
       && noexcept(bool(std::declval<_Tp>() < std::declval<_Up>()));
 }

    public:
      template<typename _Tp, __decayed_same_as<_Tp> _Up>
 requires __strongly_ordered<_Tp, _Up> || __op_eq_lt<_Tp, _Up>
 constexpr strong_ordering
 operator() [[nodiscard]] (_Tp&& __e, _Up&& __f) const
 noexcept(_S_noexcept<_Tp, _Up>())
 {
   if constexpr (__strongly_ordered<_Tp, _Up>)
     return _Strong_order{}(static_cast<_Tp&&>(__e),
       static_cast<_Up&&>(__f));
   else
     return static_cast<_Tp&&>(__e) == static_cast<_Up&&>(__f)
       ? strong_ordering::equal
       : static_cast<_Tp&&>(__e) < static_cast<_Up&&>(__f)
       ? strong_ordering::less
       : strong_ordering::greater;
 }
    };

    class _Weak_fallback
    {
      template<typename _Tp, typename _Up>
 static constexpr bool
 _S_noexcept()
 {
   if constexpr (__weakly_ordered<_Tp, _Up>)
     return _Weak_order::_S_noexcept<_Tp, _Up>();
   else
     return noexcept(bool(std::declval<_Tp>() == std::declval<_Up>()))
       && noexcept(bool(std::declval<_Tp>() < std::declval<_Up>()));
 }

    public:
      template<typename _Tp, __decayed_same_as<_Tp> _Up>
 requires __weakly_ordered<_Tp, _Up> || __op_eq_lt<_Tp, _Up>
 constexpr weak_ordering
 operator() [[nodiscard]] (_Tp&& __e, _Up&& __f) const
 noexcept(_S_noexcept<_Tp, _Up>())
 {
   if constexpr (__weakly_ordered<_Tp, _Up>)
     return _Weak_order{}(static_cast<_Tp&&>(__e),
     static_cast<_Up&&>(__f));
   else
     return static_cast<_Tp&&>(__e) == static_cast<_Up&&>(__f)
       ? weak_ordering::equivalent
       : static_cast<_Tp&&>(__e) < static_cast<_Up&&>(__f)
       ? weak_ordering::less
       : weak_ordering::greater;
 }
    };



    template<typename _Tp, typename _Up>
      concept __op_eq_lt_lt = __op_eq_lt<_Tp, _Up>
 && requires(_Tp&& __t, _Up&& __u)
 {
   { static_cast<_Up&&>(__u) < static_cast<_Tp&&>(__t) }
     -> convertible_to<bool>;
 };

    class _Partial_fallback
    {
      template<typename _Tp, typename _Up>
 static constexpr bool
 _S_noexcept()
 {
   if constexpr (__partially_ordered<_Tp, _Up>)
     return _Partial_order::_S_noexcept<_Tp, _Up>();
   else
     return noexcept(bool(std::declval<_Tp>() == std::declval<_Up>()))
       && noexcept(bool(std::declval<_Tp>() < std::declval<_Up>()));
 }

    public:
      template<typename _Tp, __decayed_same_as<_Tp> _Up>
 requires __partially_ordered<_Tp, _Up> || __op_eq_lt_lt<_Tp, _Up>
 constexpr partial_ordering
 operator() [[nodiscard]] (_Tp&& __e, _Up&& __f) const
 noexcept(_S_noexcept<_Tp, _Up>())
 {
   if constexpr (__partially_ordered<_Tp, _Up>)
     return _Partial_order{}(static_cast<_Tp&&>(__e),
        static_cast<_Up&&>(__f));
   else
     return static_cast<_Tp&&>(__e) == static_cast<_Up&&>(__f)
       ? partial_ordering::equivalent
       : static_cast<_Tp&&>(__e) < static_cast<_Up&&>(__f)
       ? partial_ordering::less
       : static_cast<_Up&&>(__f) < static_cast<_Tp&&>(__e)
       ? partial_ordering::greater
       : partial_ordering::unordered;
 }
    };
  }



  inline namespace _Cpo
  {
    inline constexpr __compare::_Strong_order strong_order{};

    inline constexpr __compare::_Weak_order weak_order{};

    inline constexpr __compare::_Partial_order partial_order{};

    inline constexpr __compare::_Strong_fallback
      compare_strong_order_fallback{};

    inline constexpr __compare::_Weak_fallback
      compare_weak_order_fallback{};

    inline constexpr __compare::_Partial_fallback
      compare_partial_order_fallback{};
  }


  namespace __detail
  {

    inline constexpr struct _Synth3way
    {
      template<typename _Tp, typename _Up>
 static constexpr bool
 _S_noexcept(const _Tp* __t = nullptr, const _Up* __u = nullptr)
 {
   if constexpr (three_way_comparable_with<_Tp, _Up>)
     return noexcept(*__t <=> *__u);
   else
     return noexcept(*__t < *__u) && noexcept(*__u < *__t);
 }

      template<typename _Tp, typename _Up>
 [[nodiscard]]
 constexpr auto
 operator()(const _Tp& __t, const _Up& __u) const
 noexcept(_S_noexcept<_Tp, _Up>())
 requires requires
 {
   { __t < __u } -> __boolean_testable;
   { __u < __t } -> __boolean_testable;
 }
 {
   if constexpr (three_way_comparable_with<_Tp, _Up>)
     return __t <=> __u;
   else
     {
       if (__t < __u)
  return weak_ordering::less;
       else if (__u < __t)
  return weak_ordering::greater;
       else
  return weak_ordering::equivalent;
     }
 }
    } __synth3way = {};


    template<typename _Tp, typename _Up = _Tp>
      using __synth3way_t
 = decltype(__detail::__synth3way(std::declval<_Tp&>(),
      std::declval<_Up&>()));
  }


}

#pragma GCC diagnostic pop
# 66 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 2 3


namespace std __attribute__ ((__visibility__ ("default")))
{

# 79 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 3
  struct piecewise_construct_t { explicit piecewise_construct_t() = default; };


  inline constexpr piecewise_construct_t piecewise_construct =
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


      constexpr pair(const pair&) = default;
      constexpr pair(pair&&) = default;

      template<typename... _Args1, typename... _Args2>
 constexpr
 pair(piecewise_construct_t, tuple<_Args1...>, tuple<_Args2...>);


      constexpr void
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
 constexpr
 pair(tuple<_Args1...>&, tuple<_Args2...>&,
      _Index_tuple<_Indexes1...>, _Index_tuple<_Indexes2...>);
    public:





      constexpr
      explicit(__not_<__and_<__is_implicitly_default_constructible<_T1>,
        __is_implicitly_default_constructible<_T2>>>())
      pair()
      noexcept(is_nothrow_default_constructible_v<_T1>
  && is_nothrow_default_constructible_v<_T2>)
      requires is_default_constructible_v<_T1>
        && is_default_constructible_v<_T2>
      : first(), second()
      { }

    private:


      template<typename _U1, typename _U2>
 static constexpr bool
 _S_constructible()
 {
   if constexpr (is_constructible_v<_T1, _U1>)
     return is_constructible_v<_T2, _U2>;
   return false;
 }

      template<typename _U1, typename _U2>
 static constexpr bool
 _S_nothrow_constructible()
 {
   if constexpr (is_nothrow_constructible_v<_T1, _U1>)
     return is_nothrow_constructible_v<_T2, _U2>;
   return false;
 }

      template<typename _U1, typename _U2>
 static constexpr bool
 _S_convertible()
 {
   if constexpr (is_convertible_v<_U1, _T1>)
     return is_convertible_v<_U2, _T2>;
   return false;
 }


      template<typename _U1, typename _U2>
 static constexpr bool
 _S_dangles()
 {

   if constexpr (__reference_constructs_from_temporary(_T1, _U1&&))
     return true;
   else
     return __reference_constructs_from_temporary(_T2, _U2&&);



 }
# 426 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 3
    public:


      constexpr explicit(!_S_convertible<const _T1&, const _T2&>())
      pair(const _T1& __x, const _T2& __y)
      noexcept(_S_nothrow_constructible<const _T1&, const _T2&>())
      requires (_S_constructible<const _T1&, const _T2&>())
      : first(__x), second(__y)
      { }





      template<typename _U1, typename _U2>

 requires (_S_constructible<_U1, _U2>()) && (!_S_dangles<_U1, _U2>())
 constexpr explicit(!_S_convertible<_U1, _U2>())
 pair(_U1&& __x, _U2&& __y)
 noexcept(_S_nothrow_constructible<_U1, _U2>())
 : first(std::forward<_U1>(__x)), second(std::forward<_U2>(__y))
 { }




      template<typename _U1, typename _U2>

 requires (_S_constructible<_U1, _U2>()) && (_S_dangles<_U1, _U2>())
 constexpr explicit(!_S_convertible<_U1, _U2>())
 pair(_U1&&, _U2&&) = delete;


      template<typename _U1, typename _U2>
 requires (_S_constructible<const _U1&, const _U2&>())
   && (!_S_dangles<_U1, _U2>())
 constexpr explicit(!_S_convertible<const _U1&, const _U2&>())
 pair(const pair<_U1, _U2>& __p)
 noexcept(_S_nothrow_constructible<const _U1&, const _U2&>())
 : first(__p.first), second(__p.second)
 { }

      template<typename _U1, typename _U2>
 requires (_S_constructible<const _U1&, const _U2&>())
       && (_S_dangles<const _U1&, const _U2&>())
 constexpr explicit(!_S_convertible<const _U1&, const _U2&>())
 pair(const pair<_U1, _U2>&) = delete;


      template<typename _U1, typename _U2>
 requires (_S_constructible<_U1, _U2>()) && (!_S_dangles<_U1, _U2>())
 constexpr explicit(!_S_convertible<_U1, _U2>())
 pair(pair<_U1, _U2>&& __p)
 noexcept(_S_nothrow_constructible<_U1, _U2>())
 : first(std::forward<_U1>(__p.first)),
   second(std::forward<_U2>(__p.second))
 { }

      template<typename _U1, typename _U2>
 requires (_S_constructible<_U1, _U2>()) && (_S_dangles<_U1, _U2>())
 constexpr explicit(!_S_convertible<_U1, _U2>())
 pair(pair<_U1, _U2>&&) = delete;
# 539 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 3
  private:

      template<typename _U1, typename _U2>
 static constexpr bool
 _S_assignable()
 {
   if constexpr (is_assignable_v<_T1&, _U1>)
     return is_assignable_v<_T2&, _U2>;
   return false;
 }

      template<typename _U1, typename _U2>
 static constexpr bool
 _S_const_assignable()
 {
   if constexpr (is_assignable_v<const _T1&, _U1>)
     return is_assignable_v<const _T2&, _U2>;
   return false;
 }

      template<typename _U1, typename _U2>
 static constexpr bool
 _S_nothrow_assignable()
 {
   if constexpr (is_nothrow_assignable_v<_T1&, _U1>)
     return is_nothrow_assignable_v<_T2&, _U2>;
   return false;
 }
# 587 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 3
  public:

      pair& operator=(const pair&) = delete;


      constexpr pair&
      operator=(const pair& __p)
      noexcept(_S_nothrow_assignable<const _T1&, const _T2&>())
      requires (_S_assignable<const _T1&, const _T2&>())
      {
 first = __p.first;
 second = __p.second;
 return *this;
      }


      constexpr pair&
      operator=(pair&& __p)
      noexcept(_S_nothrow_assignable<_T1, _T2>())
      requires (_S_assignable<_T1, _T2>())
      {
 first = std::forward<first_type>(__p.first);
 second = std::forward<second_type>(__p.second);
 return *this;
      }


      template<typename _U1, typename _U2>
 constexpr pair&
 operator=(const pair<_U1, _U2>& __p)
 noexcept(_S_nothrow_assignable<const _U1&, const _U2&>())
 requires (_S_assignable<const _U1&, const _U2&>())
 {
   first = __p.first;
   second = __p.second;
   return *this;
 }


      template<typename _U1, typename _U2>
 constexpr pair&
 operator=(pair<_U1, _U2>&& __p)
 noexcept(_S_nothrow_assignable<_U1, _U2>())
 requires (_S_assignable<_U1, _U2>())
 {
   first = std::forward<_U1>(__p.first);
   second = std::forward<_U2>(__p.second);
   return *this;
 }
# 997 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 3
    };




  template<typename _T1, typename _T2> pair(_T1, _T2) -> pair<_T1, _T2>;







  template<typename _T1, typename _T2, typename _U1, typename _U2>
    [[nodiscard]]
    constexpr bool
    operator==(const pair<_T1, _T2>& __x, const pair<_U1, _U2>& __y)
    requires requires {
      { __x.first == __y.first } -> __detail::__boolean_testable;
      { __x.second == __y.second } -> __detail::__boolean_testable;
    }
    { return __x.first == __y.first && __x.second == __y.second; }
# 1027 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 3
  template<typename _T1, typename _T2, typename _U1, typename _U2>
    [[nodiscard]]
    constexpr common_comparison_category_t<__detail::__synth3way_t<_T1, _U1>,
        __detail::__synth3way_t<_T2, _U2>>
    operator<=>(const pair<_T1, _T2>& __x, const pair<_U1, _U2>& __y)
    {
      if (auto __c = __detail::__synth3way(__x.first, __y.first); __c != 0)
 return __c;
      return __detail::__synth3way(__x.second, __y.second);
    }
# 1094 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 3
  template<typename _T1, typename _T2>
    constexpr inline


    typename enable_if<__and_<__is_swappable<_T1>,
                              __is_swappable<_T2>>::value>::type



    swap(pair<_T1, _T2>& __x, pair<_T1, _T2>& __y)
    noexcept(noexcept(__x.swap(__y)))
    { __x.swap(__y); }
# 1117 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 3
  template<typename _T1, typename _T2>
    typename enable_if<!__and_<__is_swappable<_T1>,
          __is_swappable<_T2>>::value>::type
    swap(pair<_T1, _T2>&, pair<_T1, _T2>&) = delete;
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


  template<typename _Tp1, typename _Tp2>
    inline constexpr size_t tuple_size_v<pair<_Tp1, _Tp2>> = 2;

  template<typename _Tp1, typename _Tp2>
    inline constexpr size_t tuple_size_v<const pair<_Tp1, _Tp2>> = 2;



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
# 41 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/uses_allocator.h" 1 3
# 40 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/uses_allocator.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{




  struct __erased_type { };




  template<typename _Alloc, typename _Tp>
    using __is_erased_or_convertible
      = __or_<is_convertible<_Alloc, _Tp>, is_same<_Tp, __erased_type>>;


  struct allocator_arg_t { explicit allocator_arg_t() = default; };

  inline constexpr allocator_arg_t allocator_arg =
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
    struct _Sink { void constexpr operator=(const void*) { } } _M_a;
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
    constexpr
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


  template <typename _Tp, typename _Alloc>
    inline constexpr bool uses_allocator_v =
      uses_allocator<_Tp, _Alloc>::value;



  template<typename _Alloc, typename... _Ts>
    concept __allocator_for = (uses_allocator_v<_Ts, _Alloc> && ...);


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
    inline constexpr bool __is_uses_allocator_constructible_v =
      __is_uses_allocator_constructible<_Tp, _Alloc, _Args...>::value;


  template<typename _Tp, typename _Alloc, typename... _Args>
    struct __is_nothrow_uses_allocator_constructible
    : __is_uses_allocator_predicate<is_nothrow_constructible,
        _Tp, _Alloc, _Args...>
    { };



  template<typename _Tp, typename _Alloc, typename... _Args>
    inline constexpr bool
    __is_nothrow_uses_allocator_constructible_v =
      __is_nothrow_uses_allocator_constructible<_Tp, _Alloc, _Args...>::value;


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



  template<typename _Res, typename _Callable, typename... _Args>
    constexpr enable_if_t<is_invocable_r_v<_Res, _Callable, _Args...>, _Res>
    __invoke_r(_Callable&& __fn, _Args&&... __args)
    noexcept(is_nothrow_invocable_r_v<_Res, _Callable, _Args...>)
    {
      using __result = __invoke_result<_Callable, _Args...>;
      using __type = typename __result::type;
      using __tag = typename __result::__invoke_type;
      if constexpr (is_void_v<_Res>)
 std::__invoke_impl<__type>(__tag{}, std::forward<_Callable>(__fn),
     std::forward<_Args>(__args)...);
      else
 return std::__invoke_impl<__type>(__tag{},
       std::forward<_Callable>(__fn),
       std::forward<_Args>(__args)...);
    }
# 157 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/invoke.h" 3

}
# 44 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 2 3


# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ranges_util.h" 1 3
# 34 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ranges_util.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ranges_base.h" 1 3
# 38 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ranges_base.h" 3
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
# 39 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ranges_base.h" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 1 3
# 63 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
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



  template<>
    struct __is_integer<char8_t>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };



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


  enum class byte : unsigned char;

  template<>
    struct __is_byte<byte>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };



  template<>
    struct __is_byte<char8_t>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };



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
# 661 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/cpp_type_traits.h" 3
  template<>
    struct __is_memcmp_ordered_with<std::byte, std::byte, true>
    { static constexpr bool __value = true; };

  template<typename _Tp, bool _SameSize>
    struct __is_memcmp_ordered_with<_Tp, std::byte, _SameSize>
    { static constexpr bool __value = false; };

  template<typename _Up, bool _SameSize>
    struct __is_memcmp_ordered_with<std::byte, _Up, _SameSize>
    { static constexpr bool __value = false; };



  template<typename _ValT, typename _Tp>
    constexpr bool __can_use_memchr_for_find

      = __is_byte<_ValT>::__value

   && (is_same_v<_Tp, _ValT> || is_integral_v<_Tp>);





  template<typename _Tp>
    struct __is_move_iterator
    {
      enum { __value = 0 };
      typedef __false_type __type;
    };



  template<typename _Iterator>
    constexpr
    inline _Iterator
    __miter_base(_Iterator __it)
    { return __it; }


}
}

#pragma GCC diagnostic pop
# 64 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_types.h" 1 3
# 73 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_types.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/iterator_concepts.h" 1 3
# 39 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/iterator_concepts.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ptr_traits.h" 1 3
# 39 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ptr_traits.h" 3
namespace __gnu_debug { struct _Safe_iterator_base; }


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



  template<typename _Ptr> requires requires { typename _Ptr::element_type; }
    struct __ptr_traits_elem<_Ptr, void>
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

      requires requires {
 { pointer::pointer_to(__r) } -> convertible_to<pointer>;
      }

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






      static constexpr pointer
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
# 229 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ptr_traits.h" 3
  template<typename _Tp>
    [[__gnu__::__always_inline__]]
    constexpr _Tp*
    to_address(_Tp* __ptr) noexcept
    {
      static_assert(!is_function_v<_Tp>, "std::to_address argument "
      "must not be a function pointer");
      return __ptr;
    }
# 246 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ptr_traits.h" 3
  template<typename _Ptr>
    constexpr auto
    to_address(const _Ptr& __ptr) noexcept
    {
      if constexpr (requires { pointer_traits<_Ptr>::to_address(__ptr); })
 return pointer_traits<_Ptr>::to_address(__ptr);
      else if constexpr (is_base_of_v<__gnu_debug::_Safe_iterator_base, _Ptr>)
 return std::to_address(__ptr.base().operator->());
      else
 return std::to_address(__ptr.operator->());
    }



  template<typename _Ptr>
    [[__gnu__::__always_inline__]]
    constexpr auto
    __to_address(const _Ptr& __ptr) noexcept
    { return std::to_address(__ptr); }




}
# 40 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/iterator_concepts.h" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ranges_cmp.h" 1 3
# 37 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ranges_cmp.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{


  struct __is_transparent;





  struct identity
  {
    template<typename _Tp>
      [[nodiscard]]
      constexpr _Tp&&
      operator()(_Tp&& __t) const noexcept
      { return std::forward<_Tp>(__t); }

    using is_transparent = __is_transparent;
  };


namespace ranges
{
  namespace __detail
  {



    template<typename _Tp, typename _Up>
      concept __less_builtin_ptr_cmp
 = requires (_Tp&& __t, _Up&& __u) { { __t < __u } -> same_as<bool>; }
   && convertible_to<_Tp, const volatile void*>
   && convertible_to<_Up, const volatile void*>
   && (! requires(_Tp&& __t, _Up&& __u)
       { operator<(std::forward<_Tp>(__t), std::forward<_Up>(__u)); }
       && ! requires(_Tp&& __t, _Up&& __u)
       { std::forward<_Tp>(__t).operator<(std::forward<_Up>(__u)); });
  }







  struct equal_to
  {
    template<typename _Tp, typename _Up>
      requires equality_comparable_with<_Tp, _Up>
      constexpr bool
      operator()(_Tp&& __t, _Up&& __u) const
      noexcept(noexcept(std::declval<_Tp>() == std::declval<_Up>()))
      { return std::forward<_Tp>(__t) == std::forward<_Up>(__u); }

    using is_transparent = __is_transparent;
  };


  struct not_equal_to
  {
    template<typename _Tp, typename _Up>
      requires equality_comparable_with<_Tp, _Up>
      constexpr bool
      operator()(_Tp&& __t, _Up&& __u) const
      noexcept(noexcept(std::declval<_Tp>() == std::declval<_Up>()))
      { return !equal_to{}(std::forward<_Tp>(__t), std::forward<_Up>(__u)); }

    using is_transparent = __is_transparent;
  };


  struct less
  {
    template<typename _Tp, typename _Up>
      requires totally_ordered_with<_Tp, _Up>
      constexpr bool
      operator()(_Tp&& __t, _Up&& __u) const
      noexcept(noexcept(std::declval<_Tp>() < std::declval<_Up>()))
      {
 if constexpr (__detail::__less_builtin_ptr_cmp<_Tp, _Up>)
   {
     if (std::__is_constant_evaluated())
       return __t < __u;

     auto __x = reinterpret_cast<long unsigned int>(
       static_cast<const volatile void*>(std::forward<_Tp>(__t)));
     auto __y = reinterpret_cast<long unsigned int>(
       static_cast<const volatile void*>(std::forward<_Up>(__u)));
     return __x < __y;
   }
 else
   return std::forward<_Tp>(__t) < std::forward<_Up>(__u);
      }

    using is_transparent = __is_transparent;
  };


  struct greater
  {
    template<typename _Tp, typename _Up>
      requires totally_ordered_with<_Tp, _Up>
      constexpr bool
      operator()(_Tp&& __t, _Up&& __u) const
      noexcept(noexcept(std::declval<_Up>() < std::declval<_Tp>()))
      { return less{}(std::forward<_Up>(__u), std::forward<_Tp>(__t)); }

    using is_transparent = __is_transparent;
  };


  struct greater_equal
  {
    template<typename _Tp, typename _Up>
      requires totally_ordered_with<_Tp, _Up>
      constexpr bool
      operator()(_Tp&& __t, _Up&& __u) const
      noexcept(noexcept(std::declval<_Tp>() < std::declval<_Up>()))
      { return !less{}(std::forward<_Tp>(__t), std::forward<_Up>(__u)); }

    using is_transparent = __is_transparent;
  };


  struct less_equal
  {
    template<typename _Tp, typename _Up>
      requires totally_ordered_with<_Tp, _Up>
      constexpr bool
      operator()(_Tp&& __t, _Up&& __u) const
      noexcept(noexcept(std::declval<_Up>() < std::declval<_Tp>()))
      { return !less{}(std::forward<_Up>(__u), std::forward<_Tp>(__t)); }

    using is_transparent = __is_transparent;
  };

}


}
# 41 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/iterator_concepts.h" 2 3

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wpedantic"

namespace std __attribute__ ((__visibility__ ("default")))
{

# 63 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/iterator_concepts.h" 3
  struct default_sentinel_t { };


  inline constexpr default_sentinel_t default_sentinel{};


  struct input_iterator_tag;
  struct output_iterator_tag;
  struct forward_iterator_tag;
  struct bidirectional_iterator_tag;
  struct random_access_iterator_tag;
  struct contiguous_iterator_tag;

  template<typename _Iterator>
    struct iterator_traits;

  template<typename _Tp> requires is_object_v<_Tp>
    struct iterator_traits<_Tp*>;

  template<typename _Iterator, typename>
    struct __iterator_traits;

  namespace __detail
  {
    template<typename _Tp>
      using __with_ref = _Tp&;

    template<typename _Tp>
      concept __can_reference = requires { typename __with_ref<_Tp>; };

    template<typename _Tp>
      concept __dereferenceable = requires(_Tp& __t)
 {
   { *__t } -> __can_reference;
 };
  }

  template<__detail::__dereferenceable _Tp>
    using iter_reference_t = decltype(*std::declval<_Tp&>());

  namespace ranges
  {

    namespace __imove
    {
      void iter_move() = delete;

      template<typename _Tp>
 concept __adl_imove
   = (std::__detail::__class_or_enum<remove_reference_t<_Tp>>)
   && requires(_Tp&& __t) { iter_move(static_cast<_Tp&&>(__t)); };

      struct _IterMove
      {
      private:
 template<typename _Tp>
   struct __result
   { using type = iter_reference_t<_Tp>; };

 template<typename _Tp>
   requires __adl_imove<_Tp>
   struct __result<_Tp>
   { using type = decltype(iter_move(std::declval<_Tp>())); };

 template<typename _Tp>
   requires (!__adl_imove<_Tp>)
   && is_lvalue_reference_v<iter_reference_t<_Tp>>
   struct __result<_Tp>
   { using type = remove_reference_t<iter_reference_t<_Tp>>&&; };

 template<typename _Tp>
   static constexpr bool
   _S_noexcept()
   {
     if constexpr (__adl_imove<_Tp>)
       return noexcept(iter_move(std::declval<_Tp>()));
     else
       return noexcept(*std::declval<_Tp>());
   }

      public:

 template<std::__detail::__dereferenceable _Tp>
   using __type = typename __result<_Tp>::type;

 template<std::__detail::__dereferenceable _Tp>
   [[nodiscard]]
   constexpr __type<_Tp>
   operator()(_Tp&& __e) const
   noexcept(_S_noexcept<_Tp>())
   {
     if constexpr (__adl_imove<_Tp>)
       return iter_move(static_cast<_Tp&&>(__e));
     else if constexpr (is_lvalue_reference_v<iter_reference_t<_Tp>>)
       return static_cast<__type<_Tp>>(*__e);
     else
       return *__e;
   }
      };
    }


    inline namespace _Cpo {
      inline constexpr __imove::_IterMove iter_move{};
    }
  }

  template<__detail::__dereferenceable _Tp>
    requires __detail::__can_reference<ranges::__imove::_IterMove::__type<_Tp&>>
    using iter_rvalue_reference_t = ranges::__imove::_IterMove::__type<_Tp&>;

  template<typename> struct incrementable_traits { };

  template<typename _Tp> requires is_object_v<_Tp>
    struct incrementable_traits<_Tp*>
    { using difference_type = ptrdiff_t; };

  template<typename _Iter>
    struct incrementable_traits<const _Iter>
    : incrementable_traits<_Iter> { };

  template<typename _Tp> requires requires { typename _Tp::difference_type; }
    struct incrementable_traits<_Tp>
    { using difference_type = typename _Tp::difference_type; };

  template<typename _Tp>
    requires (!requires { typename _Tp::difference_type; }
       && requires(const _Tp& __a, const _Tp& __b)
       { { __a - __b } -> integral; })
    struct incrementable_traits<_Tp>
    {
      using difference_type
 = make_signed_t<decltype(std::declval<_Tp>() - std::declval<_Tp>())>;
    };



  template<>
    struct incrementable_traits<__int128>
    { using difference_type = __int128; };

  template<>
    struct incrementable_traits<unsigned __int128>
    { using difference_type = __int128; };


  namespace __detail
  {


    template<typename _Iter>
      concept __primary_traits_iter
 = __is_base_of(__iterator_traits<_Iter, void>, iterator_traits<_Iter>);

    template<typename _Iter, typename _Tp>
      struct __iter_traits_impl
      { using type = iterator_traits<_Iter>; };

    template<typename _Iter, typename _Tp>
      requires __primary_traits_iter<_Iter>
      struct __iter_traits_impl<_Iter, _Tp>
      { using type = _Tp; };


    template<typename _Iter, typename _Tp = _Iter>
      using __iter_traits = typename __iter_traits_impl<_Iter, _Tp>::type;

    template<typename _Tp>
      using __iter_diff_t = typename
 __iter_traits<_Tp, incrementable_traits<_Tp>>::difference_type;
  }

  template<typename _Tp>
    using iter_difference_t = __detail::__iter_diff_t<remove_cvref_t<_Tp>>;

  namespace __detail
  {
    template<typename> struct __cond_value_type { };

    template<typename _Tp> requires is_object_v<_Tp>
      struct __cond_value_type<_Tp>
      { using value_type = remove_cv_t<_Tp>; };

    template<typename _Tp>
      concept __has_member_value_type
 = requires { typename _Tp::value_type; };

    template<typename _Tp>
      concept __has_member_element_type
 = requires { typename _Tp::element_type; };

  }

  template<typename> struct indirectly_readable_traits { };

  template<typename _Tp>
    struct indirectly_readable_traits<_Tp*>
    : __detail::__cond_value_type<_Tp>
    { };

  template<typename _Iter> requires is_array_v<_Iter>
    struct indirectly_readable_traits<_Iter>
    { using value_type = remove_cv_t<remove_extent_t<_Iter>>; };

  template<typename _Iter>
    struct indirectly_readable_traits<const _Iter>
    : indirectly_readable_traits<_Iter>
    { };

  template<__detail::__has_member_value_type _Tp>
    struct indirectly_readable_traits<_Tp>
    : __detail::__cond_value_type<typename _Tp::value_type>
    { };

  template<__detail::__has_member_element_type _Tp>
    struct indirectly_readable_traits<_Tp>
    : __detail::__cond_value_type<typename _Tp::element_type>
    { };



  template<__detail::__has_member_value_type _Tp>
    requires __detail::__has_member_element_type<_Tp>
    && same_as<remove_cv_t<typename _Tp::element_type>,
        remove_cv_t<typename _Tp::value_type>>
    struct indirectly_readable_traits<_Tp>
    : __detail::__cond_value_type<typename _Tp::value_type>
    { };



  template<__detail::__has_member_value_type _Tp>
    requires __detail::__has_member_element_type<_Tp>
    struct indirectly_readable_traits<_Tp>
    { };

  namespace __detail
  {
    template<typename _Tp>
      using __iter_value_t = typename
 __iter_traits<_Tp, indirectly_readable_traits<_Tp>>::value_type;
  }

  template<typename _Tp>
    using iter_value_t = __detail::__iter_value_t<remove_cvref_t<_Tp>>;

  namespace __detail
  {


    template<typename _Iter>
      concept __cpp17_iterator = requires(_Iter __it)
 {
   { *__it } -> __can_reference;
   { ++__it } -> same_as<_Iter&>;
   { *__it++ } -> __can_reference;
 } && copyable<_Iter>;

    template<typename _Iter>
      concept __cpp17_input_iterator = __cpp17_iterator<_Iter>
 && equality_comparable<_Iter>
 && requires(_Iter __it)
 {
   typename incrementable_traits<_Iter>::difference_type;
   typename indirectly_readable_traits<_Iter>::value_type;
   typename common_reference_t<iter_reference_t<_Iter>&&,
     typename indirectly_readable_traits<_Iter>::value_type&>;
   typename common_reference_t<decltype(*__it++)&&,
     typename indirectly_readable_traits<_Iter>::value_type&>;
   requires signed_integral<
     typename incrementable_traits<_Iter>::difference_type>;
 };



    template<typename _Iter>
      concept __cpp17_fwd_iterator = __cpp17_input_iterator<_Iter>
 && constructible_from<_Iter>
 && is_reference_v<iter_reference_t<_Iter>>
 && same_as<remove_cvref_t<iter_reference_t<_Iter>>,
     typename indirectly_readable_traits<_Iter>::value_type>
 && requires(_Iter __it)
 {
   { __it++ } -> convertible_to<const _Iter&>;
   { *__it++ } -> same_as<iter_reference_t<_Iter>>;
 };

    template<typename _Iter>
      concept __cpp17_bidi_iterator = __cpp17_fwd_iterator<_Iter>
 && requires(_Iter __it)
 {
   { --__it } -> same_as<_Iter&>;
   { __it-- } -> convertible_to<const _Iter&>;
   { *__it-- } -> same_as<iter_reference_t<_Iter>>;
 };

    template<typename _Iter>
      concept __cpp17_randacc_iterator = __cpp17_bidi_iterator<_Iter>
 && totally_ordered<_Iter>
 && requires(_Iter __it,
      typename incrementable_traits<_Iter>::difference_type __n)
 {
   { __it += __n } -> same_as<_Iter&>;
   { __it -= __n } -> same_as<_Iter&>;
   { __it + __n } -> same_as<_Iter>;
   { __n + __it } -> same_as<_Iter>;
   { __it - __n } -> same_as<_Iter>;
   { __it - __it } -> same_as<decltype(__n)>;
   { __it[__n] } -> convertible_to<iter_reference_t<_Iter>>;
 };

    template<typename _Iter>
      concept __iter_with_nested_types = requires {
 typename _Iter::iterator_category;
 typename _Iter::value_type;
 typename _Iter::difference_type;
 typename _Iter::reference;
      };

    template<typename _Iter>
      concept __iter_without_nested_types = !__iter_with_nested_types<_Iter>;

    template<typename _Iter>
      concept __iter_without_category
 = !requires { typename _Iter::iterator_category; };

  }

  template<typename _Iterator>
    requires __detail::__iter_with_nested_types<_Iterator>
    struct __iterator_traits<_Iterator, void>
    {
    private:
      template<typename _Iter>
 struct __ptr
 { using type = void; };

      template<typename _Iter> requires requires { typename _Iter::pointer; }
 struct __ptr<_Iter>
 { using type = typename _Iter::pointer; };

    public:
      using iterator_category = typename _Iterator::iterator_category;
      using value_type = typename _Iterator::value_type;
      using difference_type = typename _Iterator::difference_type;
      using pointer = typename __ptr<_Iterator>::type;
      using reference = typename _Iterator::reference;
    };

  template<typename _Iterator>
    requires __detail::__iter_without_nested_types<_Iterator>
       && __detail::__cpp17_input_iterator<_Iterator>
    struct __iterator_traits<_Iterator, void>
    {
    private:
      template<typename _Iter>
 struct __cat
 { using type = input_iterator_tag; };

      template<typename _Iter>
 requires requires { typename _Iter::iterator_category; }
 struct __cat<_Iter>
 { using type = typename _Iter::iterator_category; };

      template<typename _Iter>
 requires __detail::__iter_without_category<_Iter>
    && __detail::__cpp17_randacc_iterator<_Iter>
 struct __cat<_Iter>
 { using type = random_access_iterator_tag; };

      template<typename _Iter>
 requires __detail::__iter_without_category<_Iter>
    && __detail::__cpp17_bidi_iterator<_Iter>
 struct __cat<_Iter>
 { using type = bidirectional_iterator_tag; };

      template<typename _Iter>
 requires __detail::__iter_without_category<_Iter>
    && __detail::__cpp17_fwd_iterator<_Iter>
 struct __cat<_Iter>
 { using type = forward_iterator_tag; };

      template<typename _Iter>
 struct __ptr
 { using type = void; };

      template<typename _Iter> requires requires { typename _Iter::pointer; }
 struct __ptr<_Iter>
 { using type = typename _Iter::pointer; };

      template<typename _Iter>
 requires (!requires { typename _Iter::pointer; }
     && requires(_Iter& __it) { __it.operator->(); })
 struct __ptr<_Iter>
 { using type = decltype(std::declval<_Iter&>().operator->()); };

      template<typename _Iter>
 struct __ref
 { using type = iter_reference_t<_Iter>; };

      template<typename _Iter> requires requires { typename _Iter::reference; }
 struct __ref<_Iter>
 { using type = typename _Iter::reference; };

    public:
      using iterator_category = typename __cat<_Iterator>::type;
      using value_type
 = typename indirectly_readable_traits<_Iterator>::value_type;
      using difference_type
 = typename incrementable_traits<_Iterator>::difference_type;
      using pointer = typename __ptr<_Iterator>::type;
      using reference = typename __ref<_Iterator>::type;
    };

  template<typename _Iterator>
    requires __detail::__iter_without_nested_types<_Iterator>
       && __detail::__cpp17_iterator<_Iterator>
    struct __iterator_traits<_Iterator, void>
    {
    private:
      template<typename _Iter>
 struct __diff
 { using type = void; };

      template<typename _Iter>
 requires requires
 { typename incrementable_traits<_Iter>::difference_type; }
 struct __diff<_Iter>
 {
   using type = typename incrementable_traits<_Iter>::difference_type;
 };

    public:
      using iterator_category = output_iterator_tag;
      using value_type = void;
      using difference_type = typename __diff<_Iterator>::type;
      using pointer = void;
      using reference = void;
    };

  namespace __detail
  {
    template<typename _Iter>
      struct __iter_concept_impl;


    template<typename _Iter>
      requires requires { typename __iter_traits<_Iter>::iterator_concept; }
      struct __iter_concept_impl<_Iter>
      { using type = typename __iter_traits<_Iter>::iterator_concept; };


    template<typename _Iter>
      requires (!requires { typename __iter_traits<_Iter>::iterator_concept; }
   && requires { typename __iter_traits<_Iter>::iterator_category; })
      struct __iter_concept_impl<_Iter>
      { using type = typename __iter_traits<_Iter>::iterator_category; };


    template<typename _Iter>
      requires (!requires { typename __iter_traits<_Iter>::iterator_concept; }
   && !requires { typename __iter_traits<_Iter>::iterator_category; }
   && __primary_traits_iter<_Iter>)
      struct __iter_concept_impl<_Iter>
      { using type = random_access_iterator_tag; };


    template<typename _Iter>
      struct __iter_concept_impl
      { };


    template<typename _Iter>
      using __iter_concept = typename __iter_concept_impl<_Iter>::type;

  template<typename _In>
    concept __indirectly_readable_impl = requires
      {
 typename iter_value_t<_In>;
 typename iter_reference_t<_In>;
 typename iter_rvalue_reference_t<_In>;
 requires same_as<iter_reference_t<const _In>,
    iter_reference_t<_In>>;
 requires same_as<iter_rvalue_reference_t<const _In>,
    iter_rvalue_reference_t<_In>>;
      }
      && common_reference_with<iter_reference_t<_In>&&, iter_value_t<_In>&>
      && common_reference_with<iter_reference_t<_In>&&,
         iter_rvalue_reference_t<_In>&&>
      && common_reference_with<iter_rvalue_reference_t<_In>&&,
          const iter_value_t<_In>&>;

  }


  template<typename _In>
    concept indirectly_readable
      = __detail::__indirectly_readable_impl<remove_cvref_t<_In>>;

  namespace __detail
  {
    template<typename _Tp>
      struct __indirect_value
      { using type = iter_value_t<_Tp>&; };


  }

  template<typename _Tp>
    using __indirect_value_t = typename __detail::__indirect_value<_Tp>::type;

  template<indirectly_readable _Tp>
    using iter_common_reference_t
      = common_reference_t<iter_reference_t<_Tp>, __indirect_value_t<_Tp>>;


  template<typename _Out, typename _Tp>
    concept indirectly_writable = requires(_Out&& __o, _Tp&& __t)
      {
 *__o = std::forward<_Tp>(__t);
 *std::forward<_Out>(__o) = std::forward<_Tp>(__t);
 const_cast<const iter_reference_t<_Out>&&>(*__o)
   = std::forward<_Tp>(__t);
 const_cast<const iter_reference_t<_Out>&&>(*std::forward<_Out>(__o))
   = std::forward<_Tp>(__t);
      };

  namespace ranges::__detail
  {
    class __max_diff_type;
    class __max_size_type;

    __extension__
    template<typename _Tp>
      concept __is_signed_int128

 = same_as<_Tp, __int128>;




    __extension__
    template<typename _Tp>
      concept __is_unsigned_int128

 = same_as<_Tp, unsigned __int128>;




    template<typename _Tp>
      concept __cv_bool = same_as<const volatile _Tp, const volatile bool>;

    template<typename _Tp>
      concept __integral_nonbool = integral<_Tp> && !__cv_bool<_Tp>;

    template<typename _Tp>
      concept __is_int128 = __is_signed_int128<_Tp> || __is_unsigned_int128<_Tp>;

    template<typename _Tp>
      concept __is_integer_like = __integral_nonbool<_Tp>
 || __is_int128<_Tp>
 || same_as<_Tp, __max_diff_type> || same_as<_Tp, __max_size_type>;

    template<typename _Tp>
      concept __is_signed_integer_like = signed_integral<_Tp>
 || __is_signed_int128<_Tp>
 || same_as<_Tp, __max_diff_type>;

  }

  namespace __detail { using ranges::__detail::__is_signed_integer_like; }


  template<typename _Iter>
    concept weakly_incrementable = movable<_Iter>
      && requires(_Iter __i)
      {
 typename iter_difference_t<_Iter>;
 requires __detail::__is_signed_integer_like<iter_difference_t<_Iter>>;
 { ++__i } -> same_as<_Iter&>;
 __i++;
      };

  template<typename _Iter>
    concept incrementable = regular<_Iter> && weakly_incrementable<_Iter>
      && requires(_Iter __i) { { __i++ } -> same_as<_Iter>; };

  template<typename _Iter>
    concept input_or_output_iterator
      = requires(_Iter __i) { { *__i } -> __detail::__can_reference; }
 && weakly_incrementable<_Iter>;

  template<typename _Sent, typename _Iter>
    concept sentinel_for = semiregular<_Sent>
      && input_or_output_iterator<_Iter>
      && __detail::__weakly_eq_cmp_with<_Sent, _Iter>;

  template<typename _Sent, typename _Iter>
    inline constexpr bool disable_sized_sentinel_for = false;

  template<typename _Sent, typename _Iter>
    concept sized_sentinel_for = sentinel_for<_Sent, _Iter>
    && !disable_sized_sentinel_for<remove_cv_t<_Sent>, remove_cv_t<_Iter>>
    && requires(const _Iter& __i, const _Sent& __s)
    {
      { __s - __i } -> same_as<iter_difference_t<_Iter>>;
      { __i - __s } -> same_as<iter_difference_t<_Iter>>;
    };

  template<typename _Iter>
    concept input_iterator = input_or_output_iterator<_Iter>
      && indirectly_readable<_Iter>
      && requires { typename __detail::__iter_concept<_Iter>; }
      && derived_from<__detail::__iter_concept<_Iter>, input_iterator_tag>;

  template<typename _Iter, typename _Tp>
    concept output_iterator = input_or_output_iterator<_Iter>
      && indirectly_writable<_Iter, _Tp>
      && requires(_Iter __i, _Tp&& __t) { *__i++ = std::forward<_Tp>(__t); };

  template<typename _Iter>
    concept forward_iterator = input_iterator<_Iter>
      && derived_from<__detail::__iter_concept<_Iter>, forward_iterator_tag>
      && incrementable<_Iter> && sentinel_for<_Iter, _Iter>;

  template<typename _Iter>
    concept bidirectional_iterator = forward_iterator<_Iter>
      && derived_from<__detail::__iter_concept<_Iter>,
        bidirectional_iterator_tag>
      && requires(_Iter __i)
      {
 { --__i } -> same_as<_Iter&>;
 { __i-- } -> same_as<_Iter>;
      };

  template<typename _Iter>
    concept random_access_iterator = bidirectional_iterator<_Iter>
      && derived_from<__detail::__iter_concept<_Iter>,
        random_access_iterator_tag>
      && totally_ordered<_Iter> && sized_sentinel_for<_Iter, _Iter>
      && requires(_Iter __i, const _Iter __j,
    const iter_difference_t<_Iter> __n)
      {
 { __i += __n } -> same_as<_Iter&>;
 { __j + __n } -> same_as<_Iter>;
 { __n + __j } -> same_as<_Iter>;
 { __i -= __n } -> same_as<_Iter&>;
 { __j - __n } -> same_as<_Iter>;
 { __j[__n] } -> same_as<iter_reference_t<_Iter>>;
      };

  template<typename _Iter>
    concept contiguous_iterator = random_access_iterator<_Iter>
      && derived_from<__detail::__iter_concept<_Iter>, contiguous_iterator_tag>
      && is_lvalue_reference_v<iter_reference_t<_Iter>>
      && same_as<iter_value_t<_Iter>, remove_cvref_t<iter_reference_t<_Iter>>>
      && requires(const _Iter& __i)
      {
 { std::to_address(__i) }
   -> same_as<add_pointer_t<iter_reference_t<_Iter>>>;
      };





  template<typename _Fn, typename _Iter>
    concept indirectly_unary_invocable = indirectly_readable<_Iter>
      && copy_constructible<_Fn> && invocable<_Fn&, __indirect_value_t<_Iter>>
      && invocable<_Fn&, iter_reference_t<_Iter>>
      && common_reference_with<invoke_result_t<_Fn&, __indirect_value_t<_Iter>>,
          invoke_result_t<_Fn&, iter_reference_t<_Iter>>>;

  template<typename _Fn, typename _Iter>
    concept indirectly_regular_unary_invocable = indirectly_readable<_Iter>
      && copy_constructible<_Fn>
      && regular_invocable<_Fn&, __indirect_value_t<_Iter>>
      && regular_invocable<_Fn&, iter_reference_t<_Iter>>
      && common_reference_with<invoke_result_t<_Fn&, __indirect_value_t<_Iter>>,
          invoke_result_t<_Fn&, iter_reference_t<_Iter>>>;

  template<typename _Fn, typename _Iter>
    concept indirect_unary_predicate = indirectly_readable<_Iter>
      && copy_constructible<_Fn> && predicate<_Fn&, __indirect_value_t<_Iter>>
      && predicate<_Fn&, iter_reference_t<_Iter>>;

  template<typename _Fn, typename _I1, typename _I2>
    concept indirect_binary_predicate
      = indirectly_readable<_I1> && indirectly_readable<_I2>
      && copy_constructible<_Fn>
      && predicate<_Fn&, __indirect_value_t<_I1>, __indirect_value_t<_I2>>
      && predicate<_Fn&, __indirect_value_t<_I1>, iter_reference_t<_I2>>
      && predicate<_Fn&, iter_reference_t<_I1>, __indirect_value_t<_I2>>
      && predicate<_Fn&, iter_reference_t<_I1>, iter_reference_t<_I2>>;

  template<typename _Fn, typename _I1, typename _I2 = _I1>
    concept indirect_equivalence_relation
      = indirectly_readable<_I1> && indirectly_readable<_I2>
      && copy_constructible<_Fn>
      && equivalence_relation<_Fn&, __indirect_value_t<_I1>, __indirect_value_t<_I2>>
      && equivalence_relation<_Fn&, __indirect_value_t<_I1>, iter_reference_t<_I2>>
      && equivalence_relation<_Fn&, iter_reference_t<_I1>, __indirect_value_t<_I2>>
      && equivalence_relation<_Fn&, iter_reference_t<_I1>,
         iter_reference_t<_I2>>;

  template<typename _Fn, typename _I1, typename _I2 = _I1>
    concept indirect_strict_weak_order
      = indirectly_readable<_I1> && indirectly_readable<_I2>
      && copy_constructible<_Fn>
      && strict_weak_order<_Fn&, __indirect_value_t<_I1>, __indirect_value_t<_I2>>
      && strict_weak_order<_Fn&, __indirect_value_t<_I1>, iter_reference_t<_I2>>
      && strict_weak_order<_Fn&, iter_reference_t<_I1>, __indirect_value_t<_I2>>
      && strict_weak_order<_Fn&, iter_reference_t<_I1>, iter_reference_t<_I2>>;

  template<typename _Fn, typename... _Is>
    requires (indirectly_readable<_Is> && ...)
      && invocable<_Fn, iter_reference_t<_Is>...>
    using indirect_result_t = invoke_result_t<_Fn, iter_reference_t<_Is>...>;

  namespace __detail
  {
    template<typename _Iter, typename _Proj>
      struct __projected
      {
 struct __type
 {
   using value_type = remove_cvref_t<indirect_result_t<_Proj&, _Iter>>;
   indirect_result_t<_Proj&, _Iter> operator*() const;



   using __projected_Iter = _Iter;
   using __projected_Proj = _Proj;
 };
      };

    template<weakly_incrementable _Iter, typename _Proj>
      struct __projected<_Iter, _Proj>
      {
 struct __type
 {
   using value_type = remove_cvref_t<indirect_result_t<_Proj&, _Iter>>;
   using difference_type = iter_difference_t<_Iter>;
   indirect_result_t<_Proj&, _Iter> operator*() const;

   using __projected_Iter = _Iter;
   using __projected_Proj = _Proj;
 };
      };


    template<typename _Iter>
      struct __projected<_Iter, identity>
      { using __type = _Iter; };
  }


  template<indirectly_readable _Iter,
    indirectly_regular_unary_invocable<_Iter> _Proj>
    using projected = typename __detail::__projected<_Iter, _Proj>::__type;


  template<typename _Tp>
    requires same_as<_Tp, projected<typename _Tp::__projected_Iter,
        typename _Tp::__projected_Proj>>
    struct __detail::__indirect_value<_Tp>
    {
      using _Iter = typename _Tp::__projected_Iter;
      using _Proj = typename _Tp::__projected_Proj;
      using type = invoke_result_t<_Proj&, __indirect_value_t<_Iter>>;
    };
# 847 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/iterator_concepts.h" 3
  template<typename _In, typename _Out>
    concept indirectly_movable = indirectly_readable<_In>
      && indirectly_writable<_Out, iter_rvalue_reference_t<_In>>;

  template<typename _In, typename _Out>
    concept indirectly_movable_storable = indirectly_movable<_In, _Out>
      && indirectly_writable<_Out, iter_value_t<_In>>
      && movable<iter_value_t<_In>>
      && constructible_from<iter_value_t<_In>, iter_rvalue_reference_t<_In>>
      && assignable_from<iter_value_t<_In>&, iter_rvalue_reference_t<_In>>;


  template<typename _In, typename _Out>
    concept indirectly_copyable = indirectly_readable<_In>
      && indirectly_writable<_Out, iter_reference_t<_In>>;

  template<typename _In, typename _Out>
    concept indirectly_copyable_storable = indirectly_copyable<_In, _Out>
      && indirectly_writable<_Out, iter_value_t<_In>&>
      && indirectly_writable<_Out, const iter_value_t<_In>&>
      && indirectly_writable<_Out, iter_value_t<_In>&&>
      && indirectly_writable<_Out, const iter_value_t<_In>&&>
      && copyable<iter_value_t<_In>>
      && constructible_from<iter_value_t<_In>, iter_reference_t<_In>>
      && assignable_from<iter_value_t<_In>&, iter_reference_t<_In>>;

namespace ranges
{

  namespace __iswap
  {
    template<typename _It1, typename _It2>
      void iter_swap(_It1, _It2) = delete;

    template<typename _Tp, typename _Up>
      concept __adl_iswap
 = (std::__detail::__class_or_enum<remove_reference_t<_Tp>>
   || std::__detail::__class_or_enum<remove_reference_t<_Up>>)
 && requires(_Tp&& __t, _Up&& __u) {
   iter_swap(static_cast<_Tp&&>(__t), static_cast<_Up&&>(__u));
 };

    template<typename _Xp, typename _Yp>
      constexpr iter_value_t<_Xp>
      __iter_exchange_move(_Xp&& __x, _Yp&& __y)
      noexcept(noexcept(iter_value_t<_Xp>(iter_move(__x)))
        && noexcept(*__x = iter_move(__y)))
      {
 iter_value_t<_Xp> __old_value(iter_move(__x));
 *__x = iter_move(__y);
 return __old_value;
      }

    struct _IterSwap
    {
    private:
      template<typename _Tp, typename _Up>
 static constexpr bool
 _S_noexcept()
 {
   if constexpr (__adl_iswap<_Tp, _Up>)
     return noexcept(iter_swap(std::declval<_Tp>(),
          std::declval<_Up>()));
   else if constexpr (indirectly_readable<_Tp>
       && indirectly_readable<_Up>
       && swappable_with<iter_reference_t<_Tp>, iter_reference_t<_Up>>)
     return noexcept(ranges::swap(*std::declval<_Tp>(),
      *std::declval<_Up>()));
   else
     return noexcept(*std::declval<_Tp>()
  = __iswap::__iter_exchange_move(std::declval<_Up>(),
          std::declval<_Tp>()));
 }

    public:
      template<typename _Tp, typename _Up>
 requires __adl_iswap<_Tp, _Up>
 || (indirectly_readable<remove_reference_t<_Tp>>
     && indirectly_readable<remove_reference_t<_Up>>
     && swappable_with<iter_reference_t<_Tp>, iter_reference_t<_Up>>)
 || (indirectly_movable_storable<_Tp, _Up>
     && indirectly_movable_storable<_Up, _Tp>)
 constexpr void
 operator()(_Tp&& __e1, _Up&& __e2) const
 noexcept(_S_noexcept<_Tp, _Up>())
 {
   if constexpr (__adl_iswap<_Tp, _Up>)
     iter_swap(static_cast<_Tp&&>(__e1), static_cast<_Up&&>(__e2));
   else if constexpr (indirectly_readable<_Tp>
       && indirectly_readable<_Up>
       && swappable_with<iter_reference_t<_Tp>, iter_reference_t<_Up>>)
     ranges::swap(*__e1, *__e2);
   else
     *__e1 = __iswap::__iter_exchange_move(__e2, __e1);
 }
    };
  }


  inline namespace _Cpo {
    inline constexpr __iswap::_IterSwap iter_swap{};
  }

}


  template<typename _I1, typename _I2 = _I1>
    concept indirectly_swappable
      = indirectly_readable<_I1> && indirectly_readable<_I2>
      && requires(const _I1 __i1, const _I2 __i2)
      {
 ranges::iter_swap(__i1, __i1);
 ranges::iter_swap(__i2, __i2);
 ranges::iter_swap(__i1, __i2);
 ranges::iter_swap(__i2, __i1);
      };


  template<typename _I1, typename _I2, typename _Rel, typename _P1 = identity,
    typename _P2 = identity>
    concept indirectly_comparable
      = indirect_binary_predicate<_Rel, projected<_I1, _P1>,
      projected<_I2, _P2>>;


  template<typename _Iter>
    concept permutable = forward_iterator<_Iter>
      && indirectly_movable_storable<_Iter, _Iter>
      && indirectly_swappable<_Iter, _Iter>;


  template<typename _I1, typename _I2, typename _Out,
    typename _Rel = ranges::less, typename _P1 = identity,
    typename _P2 = identity>
    concept mergeable = input_iterator<_I1> && input_iterator<_I2>
      && weakly_incrementable<_Out> && indirectly_copyable<_I1, _Out>
      && indirectly_copyable<_I2, _Out>
      && indirect_strict_weak_order<_Rel, projected<_I1, _P1>,
        projected<_I2, _P2>>;


  template<typename _Iter, typename _Rel = ranges::less,
    typename _Proj = identity>
    concept sortable = permutable<_Iter>
      && indirect_strict_weak_order<_Rel, projected<_Iter, _Proj>>;

  struct unreachable_sentinel_t
  {
    template<weakly_incrementable _It>
      friend constexpr bool
      operator==(unreachable_sentinel_t, const _It&) noexcept
      { return false; }
  };

  inline constexpr unreachable_sentinel_t unreachable_sentinel{};


  namespace ranges::__access
  {
    using std::__detail::__class_or_enum;

    struct _Decay_copy final
    {
      template<typename _Tp>
 constexpr decay_t<_Tp>
 operator()(_Tp&& __t) const
 noexcept(is_nothrow_convertible_v<_Tp, decay_t<_Tp>>)
 { return std::forward<_Tp>(__t); }
    } inline constexpr __decay_copy{};

    template<typename _Tp>
      concept __member_begin = requires(_Tp& __t)
 {
   { __decay_copy(__t.begin()) } -> input_or_output_iterator;
 };


    void begin() = delete;

    template<typename _Tp>
      concept __adl_begin = __class_or_enum<remove_reference_t<_Tp>>
 && requires(_Tp& __t)
 {
   { __decay_copy(begin(__t)) } -> input_or_output_iterator;
 };



    template<typename _Tp>
      requires is_array_v<_Tp> || __member_begin<_Tp&> || __adl_begin<_Tp&>
      auto
      __begin(_Tp& __t)
      {
 if constexpr (is_array_v<_Tp>)
   return __t + 0;
 else if constexpr (__member_begin<_Tp&>)
   return __t.begin();
 else
   return begin(__t);
      }
  }

  namespace __detail
  {

    template<typename _Tp>
      using __range_iter_t
 = decltype(ranges::__access::__begin(std::declval<_Tp&>()));

  }



}
#pragma GCC diagnostic pop
# 74 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_types.h" 2 3


namespace std __attribute__ ((__visibility__ ("default")))
{

# 95 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_types.h" 3
  struct input_iterator_tag { };


  struct output_iterator_tag { };


  struct forward_iterator_tag : public input_iterator_tag { };



  struct bidirectional_iterator_tag : public forward_iterator_tag { };



  struct random_access_iterator_tag : public bidirectional_iterator_tag { };



  struct contiguous_iterator_tag : public random_access_iterator_tag { };
# 127 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_types.h" 3
  template<typename _Category, typename _Tp, typename _Distance = ptrdiff_t,
           typename _Pointer = _Tp*, typename _Reference = _Tp&>
    struct [[__deprecated__]] iterator
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
# 178 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_types.h" 3
  template<typename _Iterator>
    struct iterator_traits
    : public __iterator_traits<_Iterator> { };
# 196 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_types.h" 3
  template<typename _Tp>

    requires is_object_v<_Tp>

    struct iterator_traits<_Tp*>
    {
      using iterator_concept = contiguous_iterator_tag;
      using iterator_category = random_access_iterator_tag;
      using value_type = remove_cv_t<_Tp>;
      using difference_type = ptrdiff_t;
      using pointer = _Tp*;
      using reference = _Tp&;
    };
# 237 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_types.h" 3
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


  template<typename _InIter>
    concept __has_input_iter_cat
      = is_convertible_v<__iter_category_t<_InIter>, input_iterator_tag>;


  template<typename _It,
    typename _Cat = __iter_category_t<_It>>
    struct __is_random_access_iter
      : is_base_of<random_access_iterator_tag, _Cat>
    {
      typedef is_base_of<random_access_iterator_tag, _Cat> _Base;
      enum { __value = _Base::value };
    };








}
# 65 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 2 3
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
# 230 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/type_traits.h" 3
  template<typename... _Tp>
    using __promoted_t = decltype((typename __promote<_Tp>::__type(0) + ...));



  template<typename _Tp, typename _Up>
    using __promote_2 = __promote<__promoted_t<_Tp, _Up>>;

  template<typename _Tp, typename _Up, typename _Vp>
    using __promote_3 = __promote<__promoted_t<_Tp, _Up, _Vp>>;

  template<typename _Tp, typename _Up, typename _Vp, typename _Wp>
    using __promote_4 = __promote<__promoted_t<_Tp, _Up, _Vp, _Wp>>;
# 274 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/type_traits.h" 3

}
}

#pragma GCC diagnostic pop
# 66 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 2 3
# 75 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
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



  enum class align_val_t: size_t {};


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

[[__nodiscard__]] void* operator new(std::size_t, std::align_val_t)
 
  __attribute__((__externally_visible__, __alloc_size__ (1), __alloc_align__ (2), __malloc__));
[[__nodiscard__]] void* operator new(std::size_t, std::align_val_t, const std::nothrow_t&)
  noexcept
  __attribute__((__externally_visible__, __alloc_size__ (1), __alloc_align__ (2), __malloc__));
void operator delete(void*, std::align_val_t)
  noexcept __attribute__((__externally_visible__));
void operator delete(void*, std::align_val_t, const std::nothrow_t&)
 
  noexcept __attribute__((__externally_visible__));
[[__nodiscard__]] void* operator new[](std::size_t, std::align_val_t)
 
  __attribute__((__externally_visible__, __alloc_size__ (1), __alloc_align__ (2), __malloc__));
[[__nodiscard__]] void* operator new[](std::size_t, std::align_val_t, const std::nothrow_t&)
  noexcept
  __attribute__((__externally_visible__, __alloc_size__ (1), __alloc_align__ (2), __malloc__));
void operator delete[](void*, std::align_val_t)
  noexcept __attribute__((__externally_visible__));
void operator delete[](void*, std::align_val_t, const std::nothrow_t&)
 
  noexcept __attribute__((__externally_visible__));

void operator delete(void*, std::size_t, std::align_val_t)
  noexcept __attribute__((__externally_visible__));
void operator delete[](void*, std::size_t, std::align_val_t)
  noexcept __attribute__((__externally_visible__));
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


  template<typename _Tp>
    [[nodiscard]] constexpr _Tp*
    launder(_Tp* __p) noexcept
    {
      if constexpr (__is_same(const volatile _Tp, const volatile void))
 static_assert(!__is_same(const volatile _Tp, const volatile void),
        "std::launder argument must not be a void pointer");

      else if constexpr (__is_function(_Tp))
 static_assert(!__is_function(_Tp),
        "std::launder argument must not be a function pointer");

      else
 return __builtin_launder(__p);
      return nullptr;
    }



  inline constexpr size_t hardware_destructive_interference_size = 64;
  inline constexpr size_t hardware_constructive_interference_size = 64;






  struct destroying_delete_t
  {
    explicit destroying_delete_t() = default;
  };

  inline constexpr destroying_delete_t destroying_delete{};

}

#pragma GCC visibility pop
#pragma GCC diagnostic pop
# 76 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/exception_defines.h" 1 3
# 77 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 2 3

# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_construct.h" 1 3
# 62 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_construct.h" 3
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
    inline constexpr
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
    inline constexpr void
    advance(_InputIterator& __i, _Distance __n)
    {

      typename iterator_traits<_InputIterator>::difference_type __d = __n;
      std::__advance(__i, __d, std::__iterator_category(__i));
    }



  template<typename _InputIterator>
    [[__nodiscard__]] [[__gnu__::__always_inline__]]
    inline constexpr _InputIterator
    next(_InputIterator __x, typename
  iterator_traits<_InputIterator>::difference_type __n = 1)
    {

     
      std::advance(__x, __n);
      return __x;
    }

  template<typename _BidirectionalIterator>
    [[__nodiscard__]] [[__gnu__::__always_inline__]]
    inline constexpr _BidirectionalIterator
    prev(_BidirectionalIterator __x, typename
  iterator_traits<_BidirectionalIterator>::difference_type __n = 1)
    {

     

      std::advance(__x, -__n);
      return __x;
    }




}
# 63 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_construct.h" 2 3
# 73 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_construct.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{



  template <typename _Tp>
    constexpr inline void
    destroy_at(_Tp* __location)
    {
      if constexpr (202002L > 201703L && is_array_v<_Tp>)
 {
   for (auto& __x : *__location)
     std::destroy_at(std::__addressof(__x));
 }
      else
 __location->~_Tp();
    }


  template<typename _Tp, typename... _Args>
    requires (!is_unbounded_array_v<_Tp>)
      && requires { ::new((void*)0) _Tp(std::declval<_Args>()...); }
    constexpr _Tp*
    construct_at(_Tp* __location, _Args&&... __args)
    noexcept(noexcept(::new((void*)0) _Tp(std::declval<_Args>()...)))
    {
      void* __loc = __location;


      if constexpr (is_array_v<_Tp>)
 {
   static_assert(sizeof...(_Args) == 0, "std::construct_at for array "
         "types must not use any arguments to initialize the "
         "array");
   return ::new(__loc) _Tp[1]();
 }
      else
 return ::new(__loc) _Tp(std::forward<_Args>(__args)...);
    }
# 120 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_construct.h" 3
  template<typename _Tp, typename... _Args>
    constexpr
    inline void
    _Construct(_Tp* __p, _Args&&... __args)
    {

      if (std::__is_constant_evaluated())
 {

   std::construct_at(__p, std::forward<_Args>(__args)...);
   return;
 }

      ::new(static_cast<void*>(__p)) _Tp(std::forward<_Args>(__args)...);
    }
# 146 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_construct.h" 3
  template<typename _T1>
    inline void
    _Construct_novalue(_T1* __p)
    { ::new(static_cast<void*>(__p)) _T1; }

  template<typename _ForwardIterator>
    constexpr void
    _Destroy(_ForwardIterator __first, _ForwardIterator __last);




  template<typename _Tp>
    constexpr inline void
    _Destroy(_Tp* __pointer)
    {

      std::destroy_at(__pointer);



    }

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++17-extensions"
# 199 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_construct.h" 3
  template<typename _ForwardIterator>
    constexpr inline void
    _Destroy(_ForwardIterator __first, _ForwardIterator __last)
    {
      typedef typename iterator_traits<_ForwardIterator>::value_type
                       _Value_type;


      static_assert(is_destructible<_Value_type>::value,
      "value type is destructible");
      if constexpr (!__has_trivial_destructor(_Value_type))
 for (; __first != __last; ++__first)
   std::_Destroy(std::__addressof(*__first));

      else if (std::__is_constant_evaluated())
 for (; __first != __last; ++__first)
   std::destroy_at(std::__addressof(*__first));





    }
# 255 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_construct.h" 3
  template<typename _ForwardIterator, typename _Size>
    constexpr inline _ForwardIterator
    _Destroy_n(_ForwardIterator __first, _Size __count)
    {
      typedef typename iterator_traits<_ForwardIterator>::value_type
                       _Value_type;


      static_assert(is_destructible<_Value_type>::value,
      "value type is destructible");
      if constexpr (!__has_trivial_destructor(_Value_type))
 for (; __count > 0; (void)++__first, --__count)
   std::_Destroy(std::__addressof(*__first));

      else if (std::__is_constant_evaluated())
 for (; __count > 0; (void)++__first, --__count)
   std::destroy_at(std::__addressof(*__first));

      else
 std::advance(__first, __count);
      return __first;




    }
#pragma GCC diagnostic pop


  template <typename _ForwardIterator>
    constexpr inline void
    destroy(_ForwardIterator __first, _ForwardIterator __last)
    {
      std::_Destroy(__first, __last);
    }

  template <typename _ForwardIterator, typename _Size>
    constexpr inline _ForwardIterator
    destroy_n(_ForwardIterator __first, _Size __count)
    {
      return std::_Destroy_n(__first, __count);
    }



}
# 79 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 2 3






namespace std __attribute__ ((__visibility__ ("default")))
{

# 96 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  namespace __detail
  {


    template<typename _Cat, typename _Limit, typename _Otherwise = _Cat>
      using __clamp_iter_cat
 = __conditional_t<derived_from<_Cat, _Limit>, _Limit, _Otherwise>;
  }




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




      template<typename _Iter>
 static constexpr bool __convertible = !is_same_v<_Iter, _Iterator>
     && convertible_to<const _Iter&, _Iterator>;


    protected:
      _Iterator current;

      typedef iterator_traits<_Iterator> __traits_type;

    public:
      typedef _Iterator iterator_type;
      typedef typename __traits_type::pointer pointer;




      using iterator_concept
 = __conditional_t<random_access_iterator<_Iterator>,
     random_access_iterator_tag,
     bidirectional_iterator_tag>;
      using iterator_category
 = __detail::__clamp_iter_cat<typename __traits_type::iterator_category,
         random_access_iterator_tag>;
      using value_type = iter_value_t<_Iterator>;
      using difference_type = iter_difference_t<_Iterator>;
      using reference = iter_reference_t<_Iterator>;
# 180 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
      constexpr
      reverse_iterator()
      noexcept(noexcept(_Iterator()))
      : current()
      { }




      explicit constexpr
      reverse_iterator(iterator_type __x)
      noexcept(noexcept(_Iterator(__x)))
      : current(__x)
      { }




      constexpr
      reverse_iterator(const reverse_iterator& __x)
      noexcept(noexcept(_Iterator(__x.current)))
      : current(__x.current)
      { }


      reverse_iterator& operator=(const reverse_iterator&) = default;






      template<typename _Iter>

 requires __convertible<_Iter>

 constexpr
        reverse_iterator(const reverse_iterator<_Iter>& __x)
 noexcept(noexcept(_Iterator(__x.current)))
 : current(__x.current)
 { }


      template<typename _Iter>

 requires __convertible<_Iter>
   && assignable_from<_Iterator&, const _Iter&>

 constexpr
 reverse_iterator&
 operator=(const reverse_iterator<_Iter>& __x)
 noexcept(noexcept(current = __x.current))
 {
   current = __x.current;
   return *this;
 }





      [[__nodiscard__]]
      constexpr iterator_type
      base() const
      noexcept(noexcept(_Iterator(current)))
      { return current; }
# 257 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
      [[__nodiscard__]]
      constexpr reference
      operator*() const
      {
 _Iterator __tmp = current;
 return *--__tmp;
      }






      [[__nodiscard__]]
      constexpr pointer
      operator->() const

      requires is_pointer_v<_Iterator>
 || requires(const _Iterator __i) { __i.operator->(); }

      {


 _Iterator __tmp = current;
 --__tmp;
 return _S_to_pointer(__tmp);
      }






      constexpr reverse_iterator&
      operator++()
      {
 --current;
 return *this;
      }






      constexpr reverse_iterator
      operator++(int)
      {
 reverse_iterator __tmp = *this;
 --current;
 return __tmp;
      }






      constexpr reverse_iterator&
      operator--()
      {
 ++current;
 return *this;
      }






      constexpr reverse_iterator
      operator--(int)
      {
 reverse_iterator __tmp = *this;
 ++current;
 return __tmp;
      }






      [[__nodiscard__]]
      constexpr reverse_iterator
      operator+(difference_type __n) const
      { return reverse_iterator(current - __n); }







      constexpr reverse_iterator&
      operator+=(difference_type __n)
      {
 current -= __n;
 return *this;
      }






      [[__nodiscard__]]
      constexpr reverse_iterator
      operator-(difference_type __n) const
      { return reverse_iterator(current + __n); }







      constexpr reverse_iterator&
      operator-=(difference_type __n)
      {
 current += __n;
 return *this;
      }






      [[__nodiscard__]]
      constexpr reference
      operator[](difference_type __n) const
      { return *(*this + __n); }


      [[nodiscard]]
      friend constexpr iter_rvalue_reference_t<_Iterator>
      iter_move(const reverse_iterator& __i)
      noexcept(is_nothrow_copy_constructible_v<_Iterator>
        && noexcept(ranges::iter_move(--std::declval<_Iterator&>())))
      {
 auto __tmp = __i.base();
 return ranges::iter_move(--__tmp);
      }

      template<indirectly_swappable<_Iterator> _Iter2>
 friend constexpr void
 iter_swap(const reverse_iterator& __x,
    const reverse_iterator<_Iter2>& __y)
 noexcept(is_nothrow_copy_constructible_v<_Iterator>
   && is_nothrow_copy_constructible_v<_Iter2>
   && noexcept(ranges::iter_swap(--std::declval<_Iterator&>(),
            --std::declval<_Iter2&>())))
 {
   auto __xtmp = __x.base();
   auto __ytmp = __y.base();
   ranges::iter_swap(--__xtmp, --__ytmp);
 }


    private:
      template<typename _Tp>
 static constexpr _Tp*
 _S_to_pointer(_Tp* __p)
        { return __p; }

      template<typename _Tp>
 static constexpr pointer
 _S_to_pointer(_Tp __t)
        { return __t.operator->(); }
    };
# 526 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _IteratorL, typename _IteratorR>
    [[nodiscard]]
    constexpr bool
    operator==(const reverse_iterator<_IteratorL>& __x,
        const reverse_iterator<_IteratorR>& __y)
    requires requires { { __x.base() == __y.base() } -> convertible_to<bool>; }
    { return __x.base() == __y.base(); }

  template<typename _IteratorL, typename _IteratorR>
    [[nodiscard]]
    constexpr bool
    operator!=(const reverse_iterator<_IteratorL>& __x,
        const reverse_iterator<_IteratorR>& __y)
    requires requires { { __x.base() != __y.base() } -> convertible_to<bool>; }
    { return __x.base() != __y.base(); }

  template<typename _IteratorL, typename _IteratorR>
    [[nodiscard]]
    constexpr bool
    operator<(const reverse_iterator<_IteratorL>& __x,
       const reverse_iterator<_IteratorR>& __y)
    requires requires { { __x.base() > __y.base() } -> convertible_to<bool>; }
    { return __x.base() > __y.base(); }

  template<typename _IteratorL, typename _IteratorR>
    [[nodiscard]]
    constexpr bool
    operator>(const reverse_iterator<_IteratorL>& __x,
       const reverse_iterator<_IteratorR>& __y)
    requires requires { { __x.base() < __y.base() } -> convertible_to<bool>; }
    { return __x.base() < __y.base(); }

  template<typename _IteratorL, typename _IteratorR>
    [[nodiscard]]
    constexpr bool
    operator<=(const reverse_iterator<_IteratorL>& __x,
        const reverse_iterator<_IteratorR>& __y)
    requires requires { { __x.base() >= __y.base() } -> convertible_to<bool>; }
    { return __x.base() >= __y.base(); }

  template<typename _IteratorL, typename _IteratorR>
    [[nodiscard]]
    constexpr bool
    operator>=(const reverse_iterator<_IteratorL>& __x,
        const reverse_iterator<_IteratorR>& __y)
    requires requires { { __x.base() <= __y.base() } -> convertible_to<bool>; }
    { return __x.base() <= __y.base(); }

  template<typename _IteratorL,
    three_way_comparable_with<_IteratorL> _IteratorR>
    [[nodiscard]]
    constexpr compare_three_way_result_t<_IteratorL, _IteratorR>
    operator<=>(const reverse_iterator<_IteratorL>& __x,
  const reverse_iterator<_IteratorR>& __y)
    { return __y.base() <=> __x.base(); }




  template<typename _Iterator>
    [[nodiscard]]
    constexpr bool
    operator==(const reverse_iterator<_Iterator>& __x,
        const reverse_iterator<_Iterator>& __y)
    requires requires { { __x.base() == __y.base() } -> convertible_to<bool>; }
    { return __x.base() == __y.base(); }

  template<three_way_comparable _Iterator>
    [[nodiscard]]
    constexpr compare_three_way_result_t<_Iterator, _Iterator>
    operator<=>(const reverse_iterator<_Iterator>& __x,
  const reverse_iterator<_Iterator>& __y)
    { return __y.base() <=> __x.base(); }
# 617 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _IteratorL, typename _IteratorR>
    [[__nodiscard__]]
    inline constexpr auto
    operator-(const reverse_iterator<_IteratorL>& __x,
       const reverse_iterator<_IteratorR>& __y)
    -> decltype(__y.base() - __x.base())
    { return __y.base() - __x.base(); }


  template<typename _Iterator>
    [[__nodiscard__]]
    inline constexpr reverse_iterator<_Iterator>
    operator+(typename reverse_iterator<_Iterator>::difference_type __n,
       const reverse_iterator<_Iterator>& __x)
    { return reverse_iterator<_Iterator>(__x.base() - __n); }



  template<typename _Iterator>
    inline constexpr reverse_iterator<_Iterator>
    __make_reverse_iterator(_Iterator __i)
    { return reverse_iterator<_Iterator>(__i); }





  template<typename _Iterator>
    [[__nodiscard__]]
    inline constexpr reverse_iterator<_Iterator>
    make_reverse_iterator(_Iterator __i)
    { return reverse_iterator<_Iterator>(__i); }


  template<typename _Iterator1, typename _Iterator2>
    requires (!sized_sentinel_for<_Iterator1, _Iterator2>)
    inline constexpr bool
    disable_sized_sentinel_for<reverse_iterator<_Iterator1>,
          reverse_iterator<_Iterator2>> = true;



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

      using difference_type = ptrdiff_t;



      explicit constexpr
      back_insert_iterator(_Container& __x)
      : container(std::__addressof(__x)) { }
# 714 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
      constexpr
      back_insert_iterator&
      operator=(const typename _Container::value_type& __value)
      {
 container->push_back(__value);
 return *this;
      }

      constexpr
      back_insert_iterator&
      operator=(typename _Container::value_type&& __value)
      {
 container->push_back(std::move(__value));
 return *this;
      }



      [[__nodiscard__]] constexpr
      back_insert_iterator&
      operator*()
      { return *this; }


      constexpr
      back_insert_iterator&
      operator++()
      { return *this; }


      constexpr
      back_insert_iterator
      operator++(int)
      { return *this; }
    };
# 761 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _Container>
    [[__nodiscard__]] constexpr
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

      using difference_type = ptrdiff_t;



      explicit constexpr
      front_insert_iterator(_Container& __x)
      : container(std::__addressof(__x)) { }
# 815 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
      constexpr
      front_insert_iterator&
      operator=(const typename _Container::value_type& __value)
      {
 container->push_front(__value);
 return *this;
      }

      constexpr
      front_insert_iterator&
      operator=(typename _Container::value_type&& __value)
      {
 container->push_front(std::move(__value));
 return *this;
      }



      [[__nodiscard__]] constexpr
      front_insert_iterator&
      operator*()
      { return *this; }


      constexpr
      front_insert_iterator&
      operator++()
      { return *this; }


      constexpr
      front_insert_iterator
      operator++(int)
      { return *this; }
    };
# 862 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _Container>
    [[__nodiscard__]] constexpr
    inline front_insert_iterator<_Container>
    front_inserter(_Container& __x)
    { return front_insert_iterator<_Container>(__x); }
# 882 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _Container>
    class insert_iterator
    : public iterator<output_iterator_tag, void, void, void, void>
    {

      using _Iter = std::__detail::__range_iter_t<_Container>;



    protected:
      _Container* container;
      _Iter iter;

    public:

      typedef _Container container_type;


      using difference_type = ptrdiff_t;






      constexpr
      insert_iterator(_Container& __x, _Iter __i)
      : container(std::__addressof(__x)), iter(__i) {}
# 943 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
      constexpr
      insert_iterator&
      operator=(const typename _Container::value_type& __value)
      {
 iter = container->insert(iter, __value);
 ++iter;
 return *this;
      }

      constexpr
      insert_iterator&
      operator=(typename _Container::value_type&& __value)
      {
 iter = container->insert(iter, std::move(__value));
 ++iter;
 return *this;
      }



      [[__nodiscard__]] constexpr
      insert_iterator&
      operator*()
      { return *this; }


      constexpr
      insert_iterator&
      operator++()
      { return *this; }


      constexpr
      insert_iterator&
      operator++(int)
      { return *this; }
    };

#pragma GCC diagnostic pop
# 996 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _Container>
    [[nodiscard]]
    constexpr insert_iterator<_Container>
    inserter(_Container& __x, std::__detail::__range_iter_t<_Container> __i)
    { return insert_iterator<_Container>(__x, __i); }
# 1011 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3

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


      using iterator_concept = std::__detail::__iter_concept<_Iterator>;


      __attribute__((__always_inline__))
      constexpr
      __normal_iterator() noexcept
      : _M_current() { }

      __attribute__((__always_inline__))
      explicit constexpr
      __normal_iterator(const _Iterator& __i) noexcept
      : _M_current(__i) { }




      template<typename _Iter> requires std::is_convertible_v<_Iter, _Iterator>



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
# 1178 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _IteratorL, typename _IteratorR, typename _Container>
    [[nodiscard, __gnu__::__always_inline__]]
    constexpr bool
    operator==(const __normal_iterator<_IteratorL, _Container>& __lhs,
        const __normal_iterator<_IteratorR, _Container>& __rhs)
    noexcept(noexcept(__lhs.base() == __rhs.base()))
    requires requires {
      { __lhs.base() == __rhs.base() } -> std::convertible_to<bool>;
    }
    { return __lhs.base() == __rhs.base(); }

  template<typename _IteratorL, typename _IteratorR, typename _Container>
    [[nodiscard, __gnu__::__always_inline__]]
    constexpr std::__detail::__synth3way_t<_IteratorR, _IteratorL>
    operator<=>(const __normal_iterator<_IteratorL, _Container>& __lhs,
  const __normal_iterator<_IteratorR, _Container>& __rhs)
    noexcept(noexcept(std::__detail::__synth3way(__lhs.base(), __rhs.base())))
    { return std::__detail::__synth3way(__lhs.base(), __rhs.base()); }

  template<typename _Iterator, typename _Container>
    [[nodiscard, __gnu__::__always_inline__]]
    constexpr bool
    operator==(const __normal_iterator<_Iterator, _Container>& __lhs,
        const __normal_iterator<_Iterator, _Container>& __rhs)
    noexcept(noexcept(__lhs.base() == __rhs.base()))
    requires requires {
      { __lhs.base() == __rhs.base() } -> std::convertible_to<bool>;
    }
    { return __lhs.base() == __rhs.base(); }

  template<typename _Iterator, typename _Container>
    [[nodiscard, __gnu__::__always_inline__]]
    constexpr std::__detail::__synth3way_t<_Iterator>
    operator<=>(const __normal_iterator<_Iterator, _Container>& __lhs,
  const __normal_iterator<_Iterator, _Container>& __rhs)
    noexcept(noexcept(std::__detail::__synth3way(__lhs.base(), __rhs.base())))
    { return std::__detail::__synth3way(__lhs.base(), __rhs.base()); }
# 1319 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
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

# 1365 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<semiregular _Sent>
    class move_sentinel
    {
    public:
      constexpr
      move_sentinel()
      noexcept(is_nothrow_default_constructible_v<_Sent>)
      : _M_last() { }

      constexpr explicit
      move_sentinel(_Sent __s)
      noexcept(is_nothrow_move_constructible_v<_Sent>)
      : _M_last(std::move(__s)) { }

      template<typename _S2> requires convertible_to<const _S2&, _Sent>
 constexpr
 move_sentinel(const move_sentinel<_S2>& __s)
 noexcept(is_nothrow_constructible_v<_Sent, const _S2&>)
 : _M_last(__s.base())
 { }

      template<typename _S2> requires assignable_from<_Sent&, const _S2&>
 constexpr move_sentinel&
 operator=(const move_sentinel<_S2>& __s)
 noexcept(is_nothrow_assignable_v<_Sent, const _S2&>)
 {
   _M_last = __s.base();
   return *this;
 }

      [[nodiscard]]
      constexpr _Sent
      base() const
      noexcept(is_nothrow_copy_constructible_v<_Sent>)
      { return _M_last; }

    private:
      _Sent _M_last;
    };


  namespace __detail
  {
    template<typename _Iterator>
      struct __move_iter_cat
      { };

    template<typename _Iterator>
      requires requires { typename __iter_category_t<_Iterator>; }
      struct __move_iter_cat<_Iterator>
      {
 using iterator_category
   = __clamp_iter_cat<__iter_category_t<_Iterator>,
        random_access_iterator_tag>;
      };
  }
# 1434 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _Iterator>
    class move_iterator

      : public __detail::__move_iter_cat<_Iterator>

    {
      _Iterator _M_current;

      using __traits_type = iterator_traits<_Iterator>;




      template<typename _Iter2>
 friend class move_iterator;




      template<typename _Iter2>
 static constexpr bool __convertible = !is_same_v<_Iter2, _Iterator>
     && convertible_to<const _Iter2&, _Iterator>;



      static auto
      _S_iter_concept()
      {
 if constexpr (random_access_iterator<_Iterator>)
   return random_access_iterator_tag{};
 else if constexpr (bidirectional_iterator<_Iterator>)
   return bidirectional_iterator_tag{};
 else if constexpr (forward_iterator<_Iterator>)
   return forward_iterator_tag{};
 else
   return input_iterator_tag{};
      }


    public:
      using iterator_type = _Iterator;


      using iterator_concept = decltype(_S_iter_concept());


      using value_type = iter_value_t<_Iterator>;
      using difference_type = iter_difference_t<_Iterator>;
      using pointer = _Iterator;
      using reference = iter_rvalue_reference_t<_Iterator>;
# 1498 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
      constexpr
      move_iterator()
      : _M_current() { }

      explicit constexpr
      move_iterator(iterator_type __i)
      : _M_current(std::move(__i)) { }

      template<typename _Iter>

 requires __convertible<_Iter>

 constexpr
 move_iterator(const move_iterator<_Iter>& __i)
 : _M_current(__i._M_current) { }

      template<typename _Iter>

 requires __convertible<_Iter>
   && assignable_from<_Iterator&, const _Iter&>

 constexpr
 move_iterator& operator=(const move_iterator<_Iter>& __i)
 {
   _M_current = __i._M_current;
   return *this;
 }







      [[nodiscard]]
      constexpr const iterator_type&
      base() const & noexcept
      { return _M_current; }

      [[nodiscard]]
      constexpr iterator_type
      base() &&
      { return std::move(_M_current); }


      [[__nodiscard__]]
      constexpr reference
      operator*() const

      { return ranges::iter_move(_M_current); }




      [[__nodiscard__]]
      constexpr pointer
      operator->() const
      { return _M_current; }

      constexpr move_iterator&
      operator++()
      {
 ++_M_current;
 return *this;
      }

      constexpr move_iterator
      operator++(int)
      {
 move_iterator __tmp = *this;
 ++_M_current;
 return __tmp;
      }


      constexpr void
      operator++(int) requires (!forward_iterator<_Iterator>)
      { ++_M_current; }


      constexpr move_iterator&
      operator--()
      {
 --_M_current;
 return *this;
      }

      constexpr move_iterator
      operator--(int)
      {
 move_iterator __tmp = *this;
 --_M_current;
 return __tmp;
      }

      [[__nodiscard__]]
      constexpr move_iterator
      operator+(difference_type __n) const
      { return move_iterator(_M_current + __n); }

      constexpr move_iterator&
      operator+=(difference_type __n)
      {
 _M_current += __n;
 return *this;
      }

      [[__nodiscard__]]
      constexpr move_iterator
      operator-(difference_type __n) const
      { return move_iterator(_M_current - __n); }

      constexpr move_iterator&
      operator-=(difference_type __n)
      {
 _M_current -= __n;
 return *this;
      }

      [[__nodiscard__]]
      constexpr reference
      operator[](difference_type __n) const

      { return ranges::iter_move(_M_current + __n); }





      template<sentinel_for<_Iterator> _Sent>
 [[nodiscard]]
 friend constexpr bool
 operator==(const move_iterator& __x, const move_sentinel<_Sent>& __y)
 { return __x.base() == __y.base(); }

      template<sized_sentinel_for<_Iterator> _Sent>
 [[nodiscard]]
 friend constexpr iter_difference_t<_Iterator>
 operator-(const move_sentinel<_Sent>& __x, const move_iterator& __y)
 { return __x.base() - __y.base(); }

      template<sized_sentinel_for<_Iterator> _Sent>
 [[nodiscard]]
 friend constexpr iter_difference_t<_Iterator>
 operator-(const move_iterator& __x, const move_sentinel<_Sent>& __y)
 { return __x.base() - __y.base(); }

      [[nodiscard]]
      friend constexpr iter_rvalue_reference_t<_Iterator>
      iter_move(const move_iterator& __i)
      noexcept(noexcept(ranges::iter_move(__i._M_current)))
      { return ranges::iter_move(__i._M_current); }

      template<indirectly_swappable<_Iterator> _Iter2>
 friend constexpr void
 iter_swap(const move_iterator& __x, const move_iterator<_Iter2>& __y)
 noexcept(noexcept(ranges::iter_swap(__x._M_current, __y._M_current)))
 { return ranges::iter_swap(__x._M_current, __y._M_current); }

    };

  template<typename _IteratorL, typename _IteratorR>
    [[__nodiscard__]]
    inline constexpr bool
    operator==(const move_iterator<_IteratorL>& __x,
        const move_iterator<_IteratorR>& __y)

    requires requires { { __x.base() == __y.base() } -> convertible_to<bool>; }

    { return __x.base() == __y.base(); }


  template<typename _IteratorL,
    three_way_comparable_with<_IteratorL> _IteratorR>
    [[__nodiscard__]]
    constexpr compare_three_way_result_t<_IteratorL, _IteratorR>
    operator<=>(const move_iterator<_IteratorL>& __x,
  const move_iterator<_IteratorR>& __y)
    { return __x.base() <=> __y.base(); }
# 1686 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _IteratorL, typename _IteratorR>
    [[__nodiscard__]]
    inline constexpr bool
    operator<(const move_iterator<_IteratorL>& __x,
       const move_iterator<_IteratorR>& __y)

    requires requires { { __x.base() < __y.base() } -> convertible_to<bool>; }

    { return __x.base() < __y.base(); }

  template<typename _IteratorL, typename _IteratorR>
    [[__nodiscard__]]
    inline constexpr bool
    operator<=(const move_iterator<_IteratorL>& __x,
        const move_iterator<_IteratorR>& __y)

    requires requires { { __y.base() < __x.base() } -> convertible_to<bool>; }

    { return !(__y < __x); }

  template<typename _IteratorL, typename _IteratorR>
    [[__nodiscard__]]
    inline constexpr bool
    operator>(const move_iterator<_IteratorL>& __x,
       const move_iterator<_IteratorR>& __y)

    requires requires { { __y.base() < __x.base() } -> convertible_to<bool>; }

    { return __y < __x; }

  template<typename _IteratorL, typename _IteratorR>
    [[__nodiscard__]]
    inline constexpr bool
    operator>=(const move_iterator<_IteratorL>& __x,
        const move_iterator<_IteratorR>& __y)

    requires requires { { __x.base() < __y.base() } -> convertible_to<bool>; }

    { return !(__x < __y); }




  template<typename _Iterator>
    [[__nodiscard__]]
    inline constexpr bool
    operator==(const move_iterator<_Iterator>& __x,
        const move_iterator<_Iterator>& __y)

    { return __x.base() == __y.base(); }


  template<three_way_comparable _Iterator>
    [[__nodiscard__]]
    constexpr compare_three_way_result_t<_Iterator>
    operator<=>(const move_iterator<_Iterator>& __x,
  const move_iterator<_Iterator>& __y)
    { return __x.base() <=> __y.base(); }
# 1782 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _IteratorL, typename _IteratorR>
    [[__nodiscard__]]
    inline constexpr auto
    operator-(const move_iterator<_IteratorL>& __x,
       const move_iterator<_IteratorR>& __y)
    -> decltype(__x.base() - __y.base())
    { return __x.base() - __y.base(); }

  template<typename _Iterator>
    [[__nodiscard__]]
    inline constexpr move_iterator<_Iterator>
    operator+(typename move_iterator<_Iterator>::difference_type __n,
       const move_iterator<_Iterator>& __x)

    requires requires { { __x.base() + __n } -> same_as<_Iterator>; }

    { return __x + __n; }

  template<typename _Iterator>
    [[__nodiscard__]]
    inline constexpr move_iterator<_Iterator>
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
# 1843 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _Iterator1, typename _Iterator2>
    requires (!sized_sentinel_for<_Iterator1, _Iterator2>)
    inline constexpr bool
    disable_sized_sentinel_for<move_iterator<_Iterator1>,
          move_iterator<_Iterator2>> = true;




  namespace __detail
  {
    template<typename _It>
      concept __common_iter_has_arrow = indirectly_readable<const _It>
 && (requires(const _It& __it) { __it.operator->(); }
     || is_reference_v<iter_reference_t<_It>>
     || constructible_from<iter_value_t<_It>, iter_reference_t<_It>>);

    template<typename _It>
      concept __common_iter_use_postfix_proxy
 = (!requires (_It& __i) { { *__i++ } -> __can_reference; })
   && constructible_from<iter_value_t<_It>, iter_reference_t<_It>>
   && move_constructible<iter_value_t<_It>>;
  }



  template<input_or_output_iterator _It, sentinel_for<_It> _Sent>
    requires (!same_as<_It, _Sent>) && copyable<_It>
  class common_iterator
  {
    template<typename _Tp, typename _Up>
      static constexpr bool
      _S_noexcept1()
      {
 if constexpr (is_trivially_default_constructible_v<_Tp>)
   return is_nothrow_assignable_v<_Tp&, _Up>;
 else
   return is_nothrow_constructible_v<_Tp, _Up>;
      }

    template<typename _It2, typename _Sent2>
      static constexpr bool
      _S_noexcept()
      { return _S_noexcept1<_It, _It2>() && _S_noexcept1<_Sent, _Sent2>(); }

    class __arrow_proxy
    {
      iter_value_t<_It> _M_keep;

      constexpr
      __arrow_proxy(iter_reference_t<_It>&& __x)
      : _M_keep(std::move(__x)) { }

      friend class common_iterator;

    public:
      constexpr const iter_value_t<_It>*
      operator->() const noexcept
      { return std::__addressof(_M_keep); }
    };

    class __postfix_proxy
    {
      iter_value_t<_It> _M_keep;

      constexpr
      __postfix_proxy(iter_reference_t<_It>&& __x)
      : _M_keep(std::forward<iter_reference_t<_It>>(__x)) { }

      friend class common_iterator;

    public:
      constexpr const iter_value_t<_It>&
      operator*() const noexcept
      { return _M_keep; }
    };

  public:
    constexpr
    common_iterator()
    noexcept(is_nothrow_default_constructible_v<_It>)
    requires default_initializable<_It>
    : _M_it(), _M_index(0)
    { }

    constexpr
    common_iterator(_It __i)
    noexcept(is_nothrow_move_constructible_v<_It>)
    : _M_it(std::move(__i)), _M_index(0)
    { }

    constexpr
    common_iterator(_Sent __s)
    noexcept(is_nothrow_move_constructible_v<_Sent>)
    : _M_sent(std::move(__s)), _M_index(1)
    { }

    template<typename _It2, typename _Sent2>
      requires convertible_to<const _It2&, _It>
 && convertible_to<const _Sent2&, _Sent>
      constexpr
      common_iterator(const common_iterator<_It2, _Sent2>& __x)
      noexcept(_S_noexcept<const _It2&, const _Sent2&>())
      : _M_valueless(), _M_index(__x._M_index)
      {
 do { if (__builtin_expect(!bool(__x._M_has_value()), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h", 1948, __PRETTY_FUNCTION__, "__x._M_has_value()"); } while (false);
 if (_M_index == 0)
   {
     if constexpr (is_trivially_default_constructible_v<_It>)
       _M_it = std::move(__x._M_it);
     else
       std::construct_at(std::__addressof(_M_it), __x._M_it);
   }
 else if (_M_index == 1)
   {
     if constexpr (is_trivially_default_constructible_v<_Sent>)
       _M_sent = std::move(__x._M_sent);
     else
       std::construct_at(std::__addressof(_M_sent), __x._M_sent);
   }
      }

    common_iterator(const common_iterator&) = default;

    constexpr
    common_iterator(const common_iterator& __x)
    noexcept(_S_noexcept<const _It&, const _Sent&>())
    requires (!is_trivially_copyable_v<_It> || !is_trivially_copyable_v<_Sent>)
    : _M_valueless(), _M_index(__x._M_index)
    {
      if (_M_index == 0)
 {
   if constexpr (is_trivially_default_constructible_v<_It>)
     _M_it = __x._M_it;
   else
     std::construct_at(std::__addressof(_M_it), __x._M_it);
 }
      else if (_M_index == 1)
 {
   if constexpr (is_trivially_default_constructible_v<_Sent>)
     _M_sent = __x._M_sent;
   else
     std::construct_at(std::__addressof(_M_sent), __x._M_sent);
 }
    }

    common_iterator(common_iterator&&) = default;

    constexpr
    common_iterator(common_iterator&& __x)
    noexcept(_S_noexcept<_It, _Sent>())
    requires (!is_trivially_copyable_v<_It> || !is_trivially_copyable_v<_Sent>)
    : _M_valueless(), _M_index(__x._M_index)
    {
      if (_M_index == 0)
 {
   if constexpr (is_trivially_default_constructible_v<_It>)
     _M_it = std::move(__x._M_it);
   else
     std::construct_at(std::__addressof(_M_it), std::move(__x._M_it));
 }
      else if (_M_index == 1)
 {
   if constexpr (is_trivially_default_constructible_v<_Sent>)
     _M_sent = std::move(__x._M_sent);
   else
     std::construct_at(std::__addressof(_M_sent),
         std::move(__x._M_sent));
 }
    }

    constexpr common_iterator&
    operator=(const common_iterator&) = default;

    constexpr common_iterator&
    operator=(const common_iterator& __x)
    noexcept(is_nothrow_copy_assignable_v<_It>
      && is_nothrow_copy_assignable_v<_Sent>
      && is_nothrow_copy_constructible_v<_It>
      && is_nothrow_copy_constructible_v<_Sent>)
    requires (!is_trivially_copy_assignable_v<_It>
  || !is_trivially_copy_assignable_v<_Sent>)
    {
      _M_assign(__x);
      return *this;
    }

    constexpr common_iterator&
    operator=(common_iterator&&) = default;

    constexpr common_iterator&
    operator=(common_iterator&& __x)
    noexcept(is_nothrow_move_assignable_v<_It>
      && is_nothrow_move_assignable_v<_Sent>
      && is_nothrow_move_constructible_v<_It>
      && is_nothrow_move_constructible_v<_Sent>)
    requires (!is_trivially_move_assignable_v<_It>
  || !is_trivially_move_assignable_v<_Sent>)
    {
      _M_assign(std::move(__x));
      return *this;
    }

    template<typename _It2, typename _Sent2>
      requires convertible_to<const _It2&, _It>
 && convertible_to<const _Sent2&, _Sent>
 && assignable_from<_It&, const _It2&>
 && assignable_from<_Sent&, const _Sent2&>
      constexpr common_iterator&
      operator=(const common_iterator<_It2, _Sent2>& __x)
      noexcept(is_nothrow_constructible_v<_It, const _It2&>
        && is_nothrow_constructible_v<_Sent, const _Sent2&>
        && is_nothrow_assignable_v<_It&, const _It2&>
        && is_nothrow_assignable_v<_Sent&, const _Sent2&>)
      {
 do { if (__builtin_expect(!bool(__x._M_has_value()), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h", 2058, __PRETTY_FUNCTION__, "__x._M_has_value()"); } while (false);
 _M_assign(__x);
 return *this;
      }


    ~common_iterator() = default;

    constexpr
    ~common_iterator()
      requires (!is_trivially_destructible_v<_It>
    || !is_trivially_destructible_v<_Sent>)




    {
      if (_M_index == 0)
 _M_it.~_It();
      else if (_M_index == 1)
 _M_sent.~_Sent();
    }

    [[nodiscard]]
    constexpr decltype(auto)
    operator*()
    {
      do { if (__builtin_expect(!bool(_M_index == 0), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h", 2085, __PRETTY_FUNCTION__, "_M_index == 0"); } while (false);
      return *_M_it;
    }

    [[nodiscard]]
    constexpr decltype(auto)
    operator*() const requires __detail::__dereferenceable<const _It>
    {
      do { if (__builtin_expect(!bool(_M_index == 0), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h", 2093, __PRETTY_FUNCTION__, "_M_index == 0"); } while (false);
      return *_M_it;
    }

    [[nodiscard]]
    constexpr auto
    operator->() const requires __detail::__common_iter_has_arrow<_It>
    {
      do { if (__builtin_expect(!bool(_M_index == 0), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h", 2101, __PRETTY_FUNCTION__, "_M_index == 0"); } while (false);
      if constexpr (is_pointer_v<_It> || requires { _M_it.operator->(); })
 return _M_it;
      else if constexpr (is_reference_v<iter_reference_t<_It>>)
 {
   auto&& __tmp = *_M_it;
   return std::__addressof(__tmp);
 }
      else
 return __arrow_proxy{*_M_it};
    }

    constexpr common_iterator&
    operator++()
    {
      do { if (__builtin_expect(!bool(_M_index == 0), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h", 2116, __PRETTY_FUNCTION__, "_M_index == 0"); } while (false);
      ++_M_it;
      return *this;
    }

    constexpr decltype(auto)
    operator++(int)
    {
      do { if (__builtin_expect(!bool(_M_index == 0), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h", 2124, __PRETTY_FUNCTION__, "_M_index == 0"); } while (false);
      if constexpr (forward_iterator<_It>)
 {
   common_iterator __tmp = *this;
   ++*this;
   return __tmp;
 }
      else if constexpr (!__detail::__common_iter_use_postfix_proxy<_It>)
 return _M_it++;
      else
 {
   __postfix_proxy __p(**this);
   ++*this;
   return __p;
 }
    }

    template<typename _It2, sentinel_for<_It> _Sent2>
      requires sentinel_for<_Sent, _It2>
      friend constexpr bool
      operator== [[nodiscard]] (const common_iterator& __x,
    const common_iterator<_It2, _Sent2>& __y)
      {
 switch(__x._M_index << 2 | __y._M_index)
   {
   case 0b0000:
   case 0b0101:
     return true;
   case 0b0001:
     return __x._M_it == __y._M_sent;
   case 0b0100:
     return __x._M_sent == __y._M_it;
   default:
     do { if (__builtin_expect(!bool(__x._M_has_value()), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h", 2157, __PRETTY_FUNCTION__, "__x._M_has_value()"); } while (false);
     do { if (__builtin_expect(!bool(__y._M_has_value()), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h", 2158, __PRETTY_FUNCTION__, "__y._M_has_value()"); } while (false);
     __builtin_unreachable();
   }
      }

    template<typename _It2, sentinel_for<_It> _Sent2>
      requires sentinel_for<_Sent, _It2> && equality_comparable_with<_It, _It2>
      friend constexpr bool
      operator== [[nodiscard]] (const common_iterator& __x,
    const common_iterator<_It2, _Sent2>& __y)
      {
 switch(__x._M_index << 2 | __y._M_index)
   {
   case 0b0101:
     return true;
   case 0b0000:
     return __x._M_it == __y._M_it;
   case 0b0001:
     return __x._M_it == __y._M_sent;
   case 0b0100:
     return __x._M_sent == __y._M_it;
   default:
     do { if (__builtin_expect(!bool(__x._M_has_value()), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h", 2180, __PRETTY_FUNCTION__, "__x._M_has_value()"); } while (false);
     do { if (__builtin_expect(!bool(__y._M_has_value()), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h", 2181, __PRETTY_FUNCTION__, "__y._M_has_value()"); } while (false);
     __builtin_unreachable();
   }
      }

    template<sized_sentinel_for<_It> _It2, sized_sentinel_for<_It> _Sent2>
      requires sized_sentinel_for<_Sent, _It2>
      friend constexpr iter_difference_t<_It2>
      operator- [[nodiscard]] (const common_iterator& __x,
          const common_iterator<_It2, _Sent2>& __y)
      {
 switch(__x._M_index << 2 | __y._M_index)
   {
   case 0b0101:
     return 0;
   case 0b0000:
     return __x._M_it - __y._M_it;
   case 0b0001:
     return __x._M_it - __y._M_sent;
   case 0b0100:
     return __x._M_sent - __y._M_it;
   default:
     do { if (__builtin_expect(!bool(__x._M_has_value()), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h", 2203, __PRETTY_FUNCTION__, "__x._M_has_value()"); } while (false);
     do { if (__builtin_expect(!bool(__y._M_has_value()), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h", 2204, __PRETTY_FUNCTION__, "__y._M_has_value()"); } while (false);
     __builtin_unreachable();
   }
      }

    [[nodiscard]]
    friend constexpr iter_rvalue_reference_t<_It>
    iter_move(const common_iterator& __i)
    noexcept(noexcept(ranges::iter_move(std::declval<const _It&>())))
    requires input_iterator<_It>
    {
      do { if (__builtin_expect(!bool(__i._M_index == 0), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h", 2215, __PRETTY_FUNCTION__, "__i._M_index == 0"); } while (false);
      return ranges::iter_move(__i._M_it);
    }

    template<indirectly_swappable<_It> _It2, typename _Sent2>
      friend constexpr void
      iter_swap(const common_iterator& __x,
  const common_iterator<_It2, _Sent2>& __y)
      noexcept(noexcept(ranges::iter_swap(std::declval<const _It&>(),
       std::declval<const _It2&>())))
      {
 do { if (__builtin_expect(!bool(__x._M_index == 0), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h", 2226, __PRETTY_FUNCTION__, "__x._M_index == 0"); } while (false);
 do { if (__builtin_expect(!bool(__y._M_index == 0), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h", 2227, __PRETTY_FUNCTION__, "__y._M_index == 0"); } while (false);
 return ranges::iter_swap(__x._M_it, __y._M_it);
      }

  private:
    template<input_or_output_iterator _It2, sentinel_for<_It2> _Sent2>
      requires (!same_as<_It2, _Sent2>) && copyable<_It2>
      friend class common_iterator;

    constexpr bool
    _M_has_value() const noexcept { return _M_index != _S_valueless; }

    template<typename _CIt>
      constexpr void
      _M_assign(_CIt&& __x)
      {
 if (_M_index == __x._M_index)
   {
     if (_M_index == 0)
       _M_it = std::forward<_CIt>(__x)._M_it;
     else if (_M_index == 1)
       _M_sent = std::forward<_CIt>(__x)._M_sent;
   }
 else
   {
     if (_M_index == 0)
       _M_it.~_It();
     else if (_M_index == 1)
       _M_sent.~_Sent();
     _M_index = _S_valueless;

     if (__x._M_index == 0)
       std::construct_at(std::__addressof(_M_it),
    std::forward<_CIt>(__x)._M_it);
     else if (__x._M_index == 1)
       std::construct_at(std::__addressof(_M_sent),
    std::forward<_CIt>(__x)._M_sent);
     _M_index = __x._M_index;
   }
      }

    union
    {
      _It _M_it;
      _Sent _M_sent;
      unsigned char _M_valueless;
    };
    unsigned char _M_index;

    static constexpr unsigned char _S_valueless{2};
  };

  template<typename _It, typename _Sent>
    struct incrementable_traits<common_iterator<_It, _Sent>>
    {
      using difference_type = iter_difference_t<_It>;
    };

  template<input_iterator _It, typename _Sent>
    struct iterator_traits<common_iterator<_It, _Sent>>
    {
    private:
      template<typename _Iter>
 struct __ptr
 {
   using type = void;
 };

      template<typename _Iter>
 requires __detail::__common_iter_has_arrow<_Iter>
 struct __ptr<_Iter>
 {
   using _CIter = common_iterator<_Iter, _Sent>;
   using type = decltype(std::declval<const _CIter&>().operator->());
 };

      static auto
      _S_iter_cat()
      {
 if constexpr (requires { requires derived_from<__iter_category_t<_It>,
             forward_iterator_tag>; })
   return forward_iterator_tag{};
 else
   return input_iterator_tag{};
      }

    public:
      using iterator_concept = __conditional_t<forward_iterator<_It>,
            forward_iterator_tag,
            input_iterator_tag>;
      using iterator_category = decltype(_S_iter_cat());
      using value_type = iter_value_t<_It>;
      using difference_type = iter_difference_t<_It>;
      using pointer = typename __ptr<_It>::type;
      using reference = iter_reference_t<_It>;
    };




  namespace __detail
  {
    template<typename _It>
      struct __counted_iter_value_type
      { };

    template<indirectly_readable _It>
      struct __counted_iter_value_type<_It>
      { using value_type = iter_value_t<_It>; };

    template<typename _It>
      struct __counted_iter_concept
      { };

    template<typename _It>
      requires requires { typename _It::iterator_concept; }
      struct __counted_iter_concept<_It>
      { using iterator_concept = typename _It::iterator_concept; };

    template<typename _It>
      struct __counted_iter_cat
      { };

    template<typename _It>
      requires requires { typename _It::iterator_category; }
      struct __counted_iter_cat<_It>
      { using iterator_category = typename _It::iterator_category; };
  }



  template<input_or_output_iterator _It>
    class counted_iterator
      : public __detail::__counted_iter_value_type<_It>,
 public __detail::__counted_iter_concept<_It>,
 public __detail::__counted_iter_cat<_It>
    {
    public:
      using iterator_type = _It;

      using difference_type = iter_difference_t<_It>;



      constexpr counted_iterator() requires default_initializable<_It> = default;

      constexpr
      counted_iterator(_It __i, iter_difference_t<_It> __n)
      : _M_current(std::move(__i)), _M_length(__n)
      { do { if (__builtin_expect(!bool(__n >= 0), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h", 2376, __PRETTY_FUNCTION__, "__n >= 0"); } while (false); }

      template<typename _It2>
 requires convertible_to<const _It2&, _It>
 constexpr
 counted_iterator(const counted_iterator<_It2>& __x)
 : _M_current(__x._M_current), _M_length(__x._M_length)
 { }

      template<typename _It2>
 requires assignable_from<_It&, const _It2&>
 constexpr counted_iterator&
 operator=(const counted_iterator<_It2>& __x)
 {
   _M_current = __x._M_current;
   _M_length = __x._M_length;
   return *this;
 }

      [[nodiscard]]
      constexpr const _It&
      base() const & noexcept
      { return _M_current; }

      [[nodiscard]]
      constexpr _It
      base() &&
      noexcept(is_nothrow_move_constructible_v<_It>)
      { return std::move(_M_current); }

      [[nodiscard]]
      constexpr iter_difference_t<_It>
      count() const noexcept { return _M_length; }

      [[nodiscard]]
      constexpr decltype(auto)
      operator*()
      noexcept(noexcept(*_M_current))
      {
 do { if (__builtin_expect(!bool(_M_length > 0), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h", 2415, __PRETTY_FUNCTION__, "_M_length > 0"); } while (false);
 return *_M_current;
      }

      [[nodiscard]]
      constexpr decltype(auto)
      operator*() const
      noexcept(noexcept(*_M_current))
      requires __detail::__dereferenceable<const _It>
      {
 do { if (__builtin_expect(!bool(_M_length > 0), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h", 2425, __PRETTY_FUNCTION__, "_M_length > 0"); } while (false);
 return *_M_current;
      }

      [[nodiscard]]
      constexpr auto
      operator->() const noexcept
      requires contiguous_iterator<_It>
      { return std::to_address(_M_current); }

      constexpr counted_iterator&
      operator++()
      {
 do { if (__builtin_expect(!bool(_M_length > 0), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h", 2438, __PRETTY_FUNCTION__, "_M_length > 0"); } while (false);
 ++_M_current;
 --_M_length;
 return *this;
      }

      constexpr decltype(auto)
      operator++(int)
      {
 do { if (__builtin_expect(!bool(_M_length > 0), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h", 2447, __PRETTY_FUNCTION__, "_M_length > 0"); } while (false);
 --_M_length;
 try
   {
     return _M_current++;
   } catch(...) {
     ++_M_length;
     throw;
   }
      }

      constexpr counted_iterator
      operator++(int) requires forward_iterator<_It>
      {
 auto __tmp = *this;
 ++*this;
 return __tmp;
      }

      constexpr counted_iterator&
      operator--() requires bidirectional_iterator<_It>
      {
 --_M_current;
 ++_M_length;
 return *this;
      }

      constexpr counted_iterator
      operator--(int) requires bidirectional_iterator<_It>
      {
 auto __tmp = *this;
 --*this;
 return __tmp;
      }

      [[nodiscard]]
      constexpr counted_iterator
      operator+(iter_difference_t<_It> __n) const
 requires random_access_iterator<_It>
      { return counted_iterator(_M_current + __n, _M_length - __n); }

      [[nodiscard]]
      friend constexpr counted_iterator
      operator+(iter_difference_t<_It> __n, const counted_iterator& __x)
      requires random_access_iterator<_It>
      { return __x + __n; }

      constexpr counted_iterator&
      operator+=(iter_difference_t<_It> __n)
      requires random_access_iterator<_It>
      {
 do { if (__builtin_expect(!bool(__n <= _M_length), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h", 2498, __PRETTY_FUNCTION__, "__n <= _M_length"); } while (false);
 _M_current += __n;
 _M_length -= __n;
 return *this;
      }

      [[nodiscard]]
      constexpr counted_iterator
      operator-(iter_difference_t<_It> __n) const
      requires random_access_iterator<_It>
      { return counted_iterator(_M_current - __n, _M_length + __n); }

      template<common_with<_It> _It2>
 [[nodiscard]]
 friend constexpr iter_difference_t<_It2>
 operator-(const counted_iterator& __x,
    const counted_iterator<_It2>& __y)
 { return __y._M_length - __x._M_length; }

      [[nodiscard]]
      friend constexpr iter_difference_t<_It>
      operator-(const counted_iterator& __x, default_sentinel_t)
      { return -__x._M_length; }

      [[nodiscard]]
      friend constexpr iter_difference_t<_It>
      operator-(default_sentinel_t, const counted_iterator& __y)
      { return __y._M_length; }

      constexpr counted_iterator&
      operator-=(iter_difference_t<_It> __n)
      requires random_access_iterator<_It>
      {
 do { if (__builtin_expect(!bool(-__n <= _M_length), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h", 2531, __PRETTY_FUNCTION__, "-__n <= _M_length"); } while (false);
 _M_current -= __n;
 _M_length += __n;
 return *this;
      }

      [[nodiscard]]
      constexpr decltype(auto)
      operator[](iter_difference_t<_It> __n) const
      noexcept(noexcept(_M_current[__n]))
      requires random_access_iterator<_It>
      {
 do { if (__builtin_expect(!bool(__n < _M_length), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h", 2543, __PRETTY_FUNCTION__, "__n < _M_length"); } while (false);
 return _M_current[__n];
      }

      template<common_with<_It> _It2>
 [[nodiscard]]
 friend constexpr bool
 operator==(const counted_iterator& __x,
     const counted_iterator<_It2>& __y)
 { return __x._M_length == __y._M_length; }

      [[nodiscard]]
      friend constexpr bool
      operator==(const counted_iterator& __x, default_sentinel_t)
      { return __x._M_length == 0; }

      template<common_with<_It> _It2>
 [[nodiscard]]
 friend constexpr strong_ordering
 operator<=>(const counted_iterator& __x,
      const counted_iterator<_It2>& __y)
 { return __y._M_length <=> __x._M_length; }

      [[nodiscard]]
      friend constexpr iter_rvalue_reference_t<_It>
      iter_move(const counted_iterator& __i)
      noexcept(noexcept(ranges::iter_move(__i._M_current)))
      requires input_iterator<_It>
      {
 do { if (__builtin_expect(!bool(__i._M_length > 0), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h", 2572, __PRETTY_FUNCTION__, "__i._M_length > 0"); } while (false);
 return ranges::iter_move(__i._M_current);
      }

      template<indirectly_swappable<_It> _It2>
 friend constexpr void
 iter_swap(const counted_iterator& __x,
    const counted_iterator<_It2>& __y)
 noexcept(noexcept(ranges::iter_swap(__x._M_current, __y._M_current)))
 {
   do { if (__builtin_expect(!bool(__x._M_length > 0 && __y._M_length > 0), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h", 2582, __PRETTY_FUNCTION__, "__x._M_length > 0 && __y._M_length > 0"); } while (false);
   ranges::iter_swap(__x._M_current, __y._M_current);
 }

    private:
      template<input_or_output_iterator _It2> friend class counted_iterator;

      _It _M_current = _It();
      iter_difference_t<_It> _M_length = 0;
    };

  template<input_iterator _It>
    requires same_as<__detail::__iter_traits<_It>, iterator_traits<_It>>
    struct iterator_traits<counted_iterator<_It>> : iterator_traits<_It>
    {
      using pointer = __conditional_t<contiguous_iterator<_It>,
          add_pointer_t<iter_reference_t<_It>>,
          void>;
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
    constexpr
    inline _Iterator
    __niter_base(__gnu_cxx::__normal_iterator<_Iterator, _Container> __it)
    noexcept(std::is_nothrow_copy_constructible<_Iterator>::value)
    { return __it.base(); }


  template<typename _Iterator>
    [[__nodiscard__]] __attribute__((__always_inline__))
    constexpr
    inline _Iterator
    __niter_base(_Iterator __it)
    noexcept(std::is_nothrow_copy_constructible<_Iterator>::value)
    { return __it; }
# 3027 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _Ite, typename _Seq>
    constexpr
    decltype(std::__niter_base(std::declval<_Ite>()))
    __niter_base(const ::__gnu_debug::_Safe_iterator<_Ite, _Seq,
   std::random_access_iterator_tag>&)
    noexcept(std::is_nothrow_copy_constructible<_Ite>::value);



  template<typename _Iterator>
    constexpr
    inline auto
    __niter_base(reverse_iterator<_Iterator> __it)
    -> decltype(__make_reverse_iterator(__niter_base(__it.base())))
    { return __make_reverse_iterator(__niter_base(__it.base())); }

  template<typename _Iterator>
    constexpr
    inline auto
    __niter_base(move_iterator<_Iterator> __it)
    -> decltype(make_move_iterator(__niter_base(__it.base())))
    { return make_move_iterator(__niter_base(__it.base())); }

  template<typename _Iterator>
    constexpr
    inline auto
    __miter_base(reverse_iterator<_Iterator> __it)
    -> decltype(__make_reverse_iterator(__miter_base(__it.base())))
    { return __make_reverse_iterator(__miter_base(__it.base())); }

  template<typename _Iterator>
    constexpr
    inline auto
    __miter_base(move_iterator<_Iterator> __it)
    -> decltype(__miter_base(__it.base()))
    { return __miter_base(__it.base()); }






  template<typename _From, typename _To>
    [[__nodiscard__]]
    constexpr
    inline _From
    __niter_wrap(_From __from, _To __res)
    { return __from + (std::__niter_base(__res) - std::__niter_base(__from)); }


  template<typename _Iterator>
    [[__nodiscard__]] __attribute__((__always_inline__))
    constexpr
    inline _Iterator
    __niter_wrap(const _Iterator&, _Iterator __res)
    { return __res; }






  template<typename _InputIterator>
    using __iter_key_t = remove_const_t<



      typename iterator_traits<_InputIterator>::value_type::first_type>;


  template<typename _InputIterator>
    using __iter_val_t



      = typename iterator_traits<_InputIterator>::value_type::second_type;


  template<typename _T1, typename _T2>
    struct pair;

  template<typename _InputIterator>
    using __iter_to_alloc_t
      = pair<const __iter_key_t<_InputIterator>, __iter_val_t<_InputIterator>>;



}
# 40 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ranges_base.h" 2 3
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
# 41 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ranges_base.h" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/max_size_type.h" 1 3
# 39 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/max_size_type.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/numbers" 1 3
# 37 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/numbers" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/version.h" 1 3
# 38 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/numbers" 2 3





#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wpedantic"

namespace std __attribute__ ((__visibility__ ("default")))
{








namespace numbers
{


  template<typename _Tp>
    using _Enable_if_floating = enable_if_t<is_floating_point_v<_Tp>, _Tp>;



  template<typename _Tp>
    inline constexpr _Tp e_v
      = _Enable_if_floating<_Tp>(2.718281828459045235360287471352662498L);


  template<typename _Tp>
    inline constexpr _Tp log2e_v
      = _Enable_if_floating<_Tp>(1.442695040888963407359924681001892137L);


  template<typename _Tp>
    inline constexpr _Tp log10e_v
      = _Enable_if_floating<_Tp>(0.434294481903251827651128918916605082L);


  template<typename _Tp>
    inline constexpr _Tp pi_v
      = _Enable_if_floating<_Tp>(3.141592653589793238462643383279502884L);


  template<typename _Tp>
    inline constexpr _Tp inv_pi_v
      = _Enable_if_floating<_Tp>(0.318309886183790671537767526745028724L);


  template<typename _Tp>
    inline constexpr _Tp inv_sqrtpi_v
      = _Enable_if_floating<_Tp>(0.564189583547756286948079451560772586L);


  template<typename _Tp>
    inline constexpr _Tp ln2_v
      = _Enable_if_floating<_Tp>(0.693147180559945309417232121458176568L);


  template<typename _Tp>
    inline constexpr _Tp ln10_v
      = _Enable_if_floating<_Tp>(2.302585092994045684017991454684364208L);


  template<typename _Tp>
    inline constexpr _Tp sqrt2_v
      = _Enable_if_floating<_Tp>(1.414213562373095048801688724209698079L);


  template<typename _Tp>
    inline constexpr _Tp sqrt3_v
      = _Enable_if_floating<_Tp>(1.732050807568877293527446341505872367L);


  template<typename _Tp>
    inline constexpr _Tp inv_sqrt3_v
      = _Enable_if_floating<_Tp>(0.577350269189625764509148780501957456L);


  template<typename _Tp>
    inline constexpr _Tp egamma_v
      = _Enable_if_floating<_Tp>(0.577215664901532860606512090082402431L);


  template<typename _Tp>
    inline constexpr _Tp phi_v
      = _Enable_if_floating<_Tp>(1.618033988749894848204586834365638118L);

  inline constexpr double e = e_v<double>;
  inline constexpr double log2e = log2e_v<double>;
  inline constexpr double log10e = log10e_v<double>;
  inline constexpr double pi = pi_v<double>;
  inline constexpr double inv_pi = inv_pi_v<double>;
  inline constexpr double inv_sqrtpi = inv_sqrtpi_v<double>;
  inline constexpr double ln2 = ln2_v<double>;
  inline constexpr double ln10 = ln10_v<double>;
  inline constexpr double sqrt2 = sqrt2_v<double>;
  inline constexpr double sqrt3 = sqrt3_v<double>;
  inline constexpr double inv_sqrt3 = inv_sqrt3_v<double>;
  inline constexpr double egamma = egamma_v<double>;
  inline constexpr double phi = phi_v<double>;
# 235 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/numbers" 3
}


}

#pragma GCC diagnostic pop
# 40 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/max_size_type.h" 2 3
# 50 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/max_size_type.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{


template<typename _Tp>
  struct numeric_limits;

namespace ranges
{
  namespace __detail
  {
    class __max_size_type
    {
    public:
      __max_size_type() = default;

      template<typename _Tp> requires integral<_Tp> || __is_int128<_Tp>
 constexpr
 __max_size_type(_Tp __i) noexcept
   : _M_val(__i), _M_msb(__i < 0)
 { }

      constexpr explicit
      __max_size_type(const __max_diff_type& __d) noexcept;

      template<typename _Tp> requires integral<_Tp> || __is_int128<_Tp>
 constexpr explicit
 operator _Tp() const noexcept
 { return _M_val; }

      constexpr explicit
      operator bool() const noexcept
      { return _M_val != 0 || _M_msb != 0; }

      constexpr __max_size_type
      operator+() const noexcept
      { return *this; }

      constexpr __max_size_type
      operator~() const noexcept
      { return __max_size_type{~_M_val, !_M_msb}; }

      constexpr __max_size_type
      operator-() const noexcept
      { return operator~() + 1; }

      constexpr __max_size_type&
      operator++() noexcept
      { return *this += 1; }

      constexpr __max_size_type
      operator++(int) noexcept
      {
 auto __tmp = *this;
 ++*this;
 return __tmp;
      }

      constexpr __max_size_type&
      operator--() noexcept
      { return *this -= 1; }

      constexpr __max_size_type
      operator--(int) noexcept
      {
 auto __tmp = *this;
 --*this;
 return __tmp;
      }

      constexpr __max_size_type&
      operator+=(const __max_size_type& __r) noexcept
      {
 const auto __sum = _M_val + __r._M_val;
 const bool __overflow = (__sum < _M_val);
 _M_msb = _M_msb ^ __r._M_msb ^ __overflow;
 _M_val = __sum;
 return *this;
      }

      constexpr __max_size_type&
      operator-=(const __max_size_type& __r) noexcept
      { return *this += -__r; }

      constexpr __max_size_type&
      operator*=(__max_size_type __r) noexcept
      {
 constexpr __max_size_type __threshold
   = __rep(1) << (_S_rep_bits / 2 - 1);
 if (_M_val < __threshold && __r < __threshold)


   _M_val = _M_val * __r._M_val;
 else
   {



     const bool __lsb = _M_val & 1;
     const bool __rlsb = __r._M_val & 1;
     *this >>= 1;
     __r >>= 1;
     _M_val = (2 * _M_val * __r._M_val
        + _M_val * __rlsb + __r._M_val * __lsb);
     *this <<= 1;
     *this += __rlsb * __lsb;
   }

 return *this;
      }

      constexpr __max_size_type&
      operator/=(const __max_size_type& __r) noexcept
      {
 do { if (__builtin_expect(!bool(__r != 0), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/max_size_type.h", 164, __PRETTY_FUNCTION__, "__r != 0"); } while (false);

 if (!_M_msb && !__r._M_msb) [[likely]]
   _M_val /= __r._M_val;
 else if (_M_msb && __r._M_msb)
   {
     _M_val = (_M_val >= __r._M_val);
     _M_msb = 0;
   }
 else if (!_M_msb && __r._M_msb)
   _M_val = 0;
 else if (_M_msb && !__r._M_msb)
   {




     const auto __orig = *this;
     *this >>= 1;
     _M_val /= __r._M_val;
     *this <<= 1;
     if (__orig - *this * __r >= __r)
       ++_M_val;
   }
 return *this;
      }

      constexpr __max_size_type&
      operator%=(const __max_size_type& __r) noexcept
      {
 if (!_M_msb && !__r._M_msb) [[likely]]
   _M_val %= __r._M_val;
 else
   *this -= (*this / __r) * __r;
 return *this;
      }

      constexpr __max_size_type&
      operator<<=(const __max_size_type& __r) noexcept
      {
 do { if (__builtin_expect(!bool(__r <= _S_rep_bits), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/max_size_type.h", 204, __PRETTY_FUNCTION__, "__r <= _S_rep_bits"); } while (false);
 if (__r != 0)
   {
     _M_msb = (_M_val >> (_S_rep_bits - __r._M_val)) & 1;

     if (__r._M_val == _S_rep_bits) [[unlikely]]
       _M_val = 0;
     else
       _M_val <<= __r._M_val;
   }
 return *this;
      }

      constexpr __max_size_type&
      operator>>=(const __max_size_type& __r) noexcept
      {
 do { if (__builtin_expect(!bool(__r <= _S_rep_bits), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/max_size_type.h", 220, __PRETTY_FUNCTION__, "__r <= _S_rep_bits"); } while (false);
 if (__r != 0)
   {
     if (__r._M_val == _S_rep_bits) [[unlikely]]
       _M_val = 0;
     else
       _M_val >>= __r._M_val;

     if (_M_msb) [[unlikely]]
       {
  _M_val |= __rep(1) << (_S_rep_bits - __r._M_val);
  _M_msb = 0;
       }
   }
 return *this;
      }

      constexpr __max_size_type&
      operator&=(const __max_size_type& __r) noexcept
      {
 _M_val &= __r._M_val;
 _M_msb &= __r._M_msb;
 return *this;
      }

      constexpr __max_size_type&
      operator|=(const __max_size_type& __r) noexcept
      {
 _M_val |= __r._M_val;
 _M_msb |= __r._M_msb;
 return *this;
      }

      constexpr __max_size_type&
      operator^=(const __max_size_type& __r) noexcept
      {
 _M_val ^= __r._M_val;
 _M_msb ^= __r._M_msb;
 return *this;
      }

      template<typename _Tp> requires integral<_Tp> || __is_int128<_Tp>
 friend constexpr _Tp&
 operator+=(_Tp& __a, const __max_size_type& __b) noexcept
 { return (__a = static_cast<_Tp>(__a + __b)); }

      template<typename _Tp> requires integral<_Tp> || __is_int128<_Tp>
 friend constexpr _Tp&
 operator-=(_Tp& __a, const __max_size_type& __b) noexcept
 { return (__a = static_cast<_Tp>(__a - __b)); }

      template<typename _Tp> requires integral<_Tp> || __is_int128<_Tp>
 friend constexpr _Tp&
 operator*=(_Tp& __a, const __max_size_type& __b) noexcept
 { return (__a = static_cast<_Tp>(__a * __b)); }

      template<typename _Tp> requires integral<_Tp> || __is_int128<_Tp>
 friend constexpr _Tp&
 operator/=(_Tp& __a, const __max_size_type& __b) noexcept
 { return (__a = static_cast<_Tp>(__a / __b)); }

      template<typename _Tp> requires integral<_Tp> || __is_int128<_Tp>
 friend constexpr _Tp&
 operator%=(_Tp& __a, const __max_size_type& __b) noexcept
 { return (__a = static_cast<_Tp>(__a % __b)); }

      template<typename _Tp> requires integral<_Tp> || __is_int128<_Tp>
 friend constexpr _Tp&
 operator&=(_Tp& __a, const __max_size_type& __b) noexcept
 { return (__a = static_cast<_Tp>(__a & __b)); }

      template<typename _Tp> requires integral<_Tp> || __is_int128<_Tp>
 friend constexpr _Tp&
 operator|=(_Tp& __a, const __max_size_type& __b) noexcept
 { return (__a = static_cast<_Tp>(__a | __b)); }

      template<typename _Tp> requires integral<_Tp> || __is_int128<_Tp>
 friend constexpr _Tp&
 operator^=(_Tp& __a, const __max_size_type& __b) noexcept
 { return (__a = static_cast<_Tp>(__a ^ __b)); }

      template<typename _Tp> requires integral<_Tp> || __is_int128<_Tp>
 friend constexpr _Tp&
 operator<<=(_Tp& __a, const __max_size_type& __b) noexcept
 { return (__a = static_cast<_Tp>(__a << __b)); }

      template<typename _Tp> requires integral<_Tp> || __is_int128<_Tp>
 friend constexpr _Tp&
 operator>>=(_Tp& __a, const __max_size_type& __b) noexcept
 { return (__a = static_cast<_Tp>(__a >> __b)); }

      friend constexpr __max_size_type
      operator+(__max_size_type __l, const __max_size_type& __r) noexcept
      {
 __l += __r;
 return __l;
      }

      friend constexpr __max_size_type
      operator-(__max_size_type __l, const __max_size_type& __r) noexcept
      {
 __l -= __r;
 return __l;
      }

      friend constexpr __max_size_type
      operator*(__max_size_type __l, const __max_size_type& __r) noexcept
      {
 __l *= __r;
 return __l;
      }

      friend constexpr __max_size_type
      operator/(__max_size_type __l, const __max_size_type& __r) noexcept
      {
 __l /= __r;
 return __l;
      }

      friend constexpr __max_size_type
      operator%(__max_size_type __l, const __max_size_type& __r) noexcept
      {
 __l %= __r;
 return __l;
      }

      friend constexpr __max_size_type
      operator<<(__max_size_type __l, const __max_size_type& __r) noexcept
      {
 __l <<= __r;
 return __l;
      }

      friend constexpr __max_size_type
      operator>>(__max_size_type __l, const __max_size_type& __r) noexcept
      {
 __l >>= __r;
 return __l;
      }

      friend constexpr __max_size_type
      operator&(__max_size_type __l, const __max_size_type& __r) noexcept
      {
 __l &= __r;
 return __l;
      }

      friend constexpr __max_size_type
      operator|(__max_size_type __l, const __max_size_type& __r) noexcept
      {
 __l |= __r;
 return __l;
      }

      friend constexpr __max_size_type
      operator^(__max_size_type __l, const __max_size_type& __r) noexcept
      {
 __l ^= __r;
 return __l;
      }

      friend constexpr bool
      operator==(const __max_size_type& __l, const __max_size_type& __r) noexcept
      { return __l._M_val == __r._M_val && __l._M_msb == __r._M_msb; }


      friend constexpr strong_ordering
      operator<=>(const __max_size_type& __l, const __max_size_type& __r) noexcept
      {
 if (__l._M_msb ^ __r._M_msb)
   return __l._M_msb ? strong_ordering::greater : strong_ordering::less;
 else
   return __l._M_val <=> __r._M_val;
      }
# 422 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/max_size_type.h" 3
      __extension__
      using __rep = unsigned __int128;



      static constexpr size_t _S_rep_bits = sizeof(__rep) * 8;
    private:
      __rep _M_val = 0;
      unsigned _M_msb:1 = 0;

      constexpr explicit
      __max_size_type(__rep __val, int __msb) noexcept
 : _M_val(__val), _M_msb(__msb)
      { }

      friend __max_diff_type;
      friend std::numeric_limits<__max_size_type>;
      friend std::numeric_limits<__max_diff_type>;
    };

    class __max_diff_type
    {
    public:
      __max_diff_type() = default;

      template<typename _Tp> requires integral<_Tp> || __is_int128<_Tp>
 constexpr
 __max_diff_type(_Tp __i) noexcept
   : _M_rep(__i)
 { }

      constexpr explicit
      __max_diff_type(const __max_size_type& __d) noexcept
 : _M_rep(__d)
      { }

      template<typename _Tp> requires integral<_Tp> || __is_int128<_Tp>
 constexpr explicit
 operator _Tp() const noexcept
 { return static_cast<_Tp>(_M_rep); }

      constexpr explicit
      operator bool() const noexcept
      { return _M_rep != 0; }

      constexpr __max_diff_type
      operator+() const noexcept
      { return *this; }

      constexpr __max_diff_type
      operator-() const noexcept
      { return __max_diff_type(-_M_rep); }

      constexpr __max_diff_type
      operator~() const noexcept
      { return __max_diff_type(~_M_rep); }

      constexpr __max_diff_type&
      operator++() noexcept
      { return *this += 1; }

      constexpr __max_diff_type
      operator++(int) noexcept
      {
 auto __tmp = *this;
 ++*this;
 return __tmp;
      }

      constexpr __max_diff_type&
      operator--() noexcept
      { return *this -= 1; }

      constexpr __max_diff_type
      operator--(int) noexcept
      {
 auto __tmp = *this;
 --*this;
 return __tmp;
      }

      constexpr __max_diff_type&
      operator+=(const __max_diff_type& __r) noexcept
      {
 _M_rep += __r._M_rep;
 return *this;
      }

      constexpr __max_diff_type&
      operator-=(const __max_diff_type& __r) noexcept
      {
 _M_rep -= __r._M_rep;
 return *this;
      }

      constexpr __max_diff_type&
      operator*=(const __max_diff_type& __r) noexcept
      {
 _M_rep *= __r._M_rep;
 return *this;
      }

      constexpr __max_diff_type&
      operator/=(const __max_diff_type& __r) noexcept
      {
 do { if (__builtin_expect(!bool(__r != 0), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/max_size_type.h", 527, __PRETTY_FUNCTION__, "__r != 0"); } while (false);
 const bool __neg = *this < 0;
 const bool __rneg = __r < 0;
 if (!__neg && !__rneg)
   _M_rep = _M_rep / __r._M_rep;
 else if (__neg && __rneg)
   _M_rep = -_M_rep / -__r._M_rep;
 else if (__neg && !__rneg)
   _M_rep = -(-_M_rep / __r._M_rep);
 else
   _M_rep = -(_M_rep / -__r._M_rep);
 return *this ;
      }

      constexpr __max_diff_type&
      operator%=(const __max_diff_type& __r) noexcept
      {
 do { if (__builtin_expect(!bool(__r != 0), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/max_size_type.h", 544, __PRETTY_FUNCTION__, "__r != 0"); } while (false);
 if (*this >= 0 && __r > 0)
   _M_rep %= __r._M_rep;
 else
   *this -= (*this / __r) * __r;
 return *this;
      }

      constexpr __max_diff_type&
      operator<<=(const __max_diff_type& __r) noexcept
      {
 _M_rep.operator<<=(__r._M_rep);
 return *this;
      }

      constexpr __max_diff_type&
      operator>>=(const __max_diff_type& __r) noexcept
      {

 const auto __msb = _M_rep._M_msb;
 _M_rep >>= __r._M_rep;
 if (__msb)
   _M_rep |= ~(__max_size_type(-1) >> __r._M_rep);
 return *this;
      }

      constexpr __max_diff_type&
      operator&=(const __max_diff_type& __r) noexcept
      {
 _M_rep &= __r._M_rep;
 return *this;
      }

      constexpr __max_diff_type&
      operator|=(const __max_diff_type& __r) noexcept
      {
 _M_rep |= __r._M_rep;
 return *this;
      }

      constexpr __max_diff_type&
      operator^=(const __max_diff_type& __r) noexcept
      {
 _M_rep ^= __r._M_rep;
 return *this;
      }

      template<typename _Tp> requires integral<_Tp> || __is_int128<_Tp>
 friend constexpr _Tp&
 operator+=(_Tp& __a, const __max_diff_type& __b) noexcept
 { return (__a = static_cast<_Tp>(__a + __b)); }

      template<typename _Tp> requires integral<_Tp> || __is_int128<_Tp>
 friend constexpr _Tp&
 operator-=(_Tp& __a, const __max_diff_type& __b) noexcept
 { return (__a = static_cast<_Tp>(__a - __b)); }

      template<typename _Tp> requires integral<_Tp> || __is_int128<_Tp>
 friend constexpr _Tp&
 operator*=(_Tp& __a, const __max_diff_type& __b) noexcept
 { return (__a = static_cast<_Tp>(__a * __b)); }

      template<typename _Tp> requires integral<_Tp> || __is_int128<_Tp>
 friend constexpr _Tp&
 operator/=(_Tp& __a, const __max_diff_type& __b) noexcept
 { return (__a = static_cast<_Tp>(__a / __b)); }

      template<typename _Tp> requires integral<_Tp> || __is_int128<_Tp>
 friend constexpr _Tp&
 operator%=(_Tp& __a, const __max_diff_type& __b) noexcept
 { return (__a = static_cast<_Tp>(__a % __b)); }

      template<typename _Tp> requires integral<_Tp> || __is_int128<_Tp>
 friend constexpr _Tp&
 operator&=(_Tp& __a, const __max_diff_type& __b) noexcept
 { return (__a = static_cast<_Tp>(__a & __b)); }

      template<typename _Tp> requires integral<_Tp> || __is_int128<_Tp>
 friend constexpr _Tp&
 operator|=(_Tp& __a, const __max_diff_type& __b) noexcept
 { return (__a = static_cast<_Tp>(__a | __b)); }

      template<typename _Tp> requires integral<_Tp> || __is_int128<_Tp>
 friend constexpr _Tp&
 operator^=(_Tp& __a, const __max_diff_type& __b) noexcept
 { return (__a = static_cast<_Tp>(__a ^ __b)); }

      template<typename _Tp> requires integral<_Tp> || __is_int128<_Tp>
 friend constexpr _Tp&
 operator<<=(_Tp& __a, const __max_diff_type& __b) noexcept
 { return (__a = static_cast<_Tp>(__a << __b)); }

      template<typename _Tp> requires integral<_Tp> || __is_int128<_Tp>
 friend constexpr _Tp&
 operator>>=(_Tp& __a, const __max_diff_type& __b) noexcept
 { return (__a = static_cast<_Tp>(__a >> __b)); }

      friend constexpr __max_diff_type
      operator+(__max_diff_type __l, const __max_diff_type& __r) noexcept
      {
 __l += __r;
 return __l;
      }

      friend constexpr __max_diff_type
      operator-(__max_diff_type __l, const __max_diff_type& __r) noexcept
      {
 __l -= __r;
 return __l;
      }

      friend constexpr __max_diff_type
      operator*(__max_diff_type __l, const __max_diff_type& __r) noexcept
      {
 __l *= __r;
 return __l;
      }

      friend constexpr __max_diff_type
      operator/(__max_diff_type __l, const __max_diff_type& __r) noexcept
      {
 __l /= __r;
 return __l;
      }

      friend constexpr __max_diff_type
      operator%(__max_diff_type __l, const __max_diff_type& __r) noexcept
      {
 __l %= __r;
 return __l;
      }

      friend constexpr __max_diff_type
      operator<<(__max_diff_type __l, const __max_diff_type& __r) noexcept
      {
 __l <<= __r;
 return __l;
      }

      friend constexpr __max_diff_type
      operator>>(__max_diff_type __l, const __max_diff_type& __r) noexcept
      {
 __l >>= __r;
 return __l;
      }

      friend constexpr __max_diff_type
      operator&(__max_diff_type __l, const __max_diff_type& __r) noexcept
      {
 __l &= __r;
 return __l;
      }

      friend constexpr __max_diff_type
      operator|(__max_diff_type __l, const __max_diff_type& __r) noexcept
      {
 __l |= __r;
 return __l;
      }

      friend constexpr __max_diff_type
      operator^(__max_diff_type __l, const __max_diff_type& __r) noexcept
      {
 __l ^= __r;
 return __l;
      }

      friend constexpr bool
      operator==(const __max_diff_type& __l, const __max_diff_type& __r) noexcept
      { return __l._M_rep == __r._M_rep; }


      constexpr strong_ordering
      operator<=>(const __max_diff_type& __r) const noexcept
      {
 const auto __lsign = _M_rep._M_msb;
 const auto __rsign = __r._M_rep._M_msb;
 if (__lsign ^ __rsign)
   return __lsign ? strong_ordering::less : strong_ordering::greater;
 else
   return _M_rep <=> __r._M_rep;
      }
# 755 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/max_size_type.h" 3
    private:
      __max_size_type _M_rep = 0;

      friend class __max_size_type;
    };

    constexpr
    __max_size_type::__max_size_type(const __max_diff_type& __d) noexcept
      : __max_size_type(__d._M_rep)
    { }

  }
}

  template<>
    struct numeric_limits<ranges::__detail::__max_size_type>
    {
      using _Sp = ranges::__detail::__max_size_type;
      static constexpr bool is_specialized = true;
      static constexpr bool is_signed = false;
      static constexpr bool is_integer = true;
      static constexpr bool is_exact = true;
      static constexpr int digits
 = __gnu_cxx::__int_traits<_Sp::__rep>::__digits + 1;
      static constexpr int digits10
 = static_cast<int>(digits * numbers::ln2 / numbers::ln10);

      static constexpr _Sp
      min() noexcept
      { return 0; }

      static constexpr _Sp
      max() noexcept
      { return _Sp(static_cast<_Sp::__rep>(-1), 1); }

      static constexpr _Sp
      lowest() noexcept
      { return min(); }
    };

  template<>
    struct numeric_limits<ranges::__detail::__max_diff_type>
    {
      using _Dp = ranges::__detail::__max_diff_type;
      using _Sp = ranges::__detail::__max_size_type;
      static constexpr bool is_specialized = true;
      static constexpr bool is_signed = true;
      static constexpr bool is_integer = true;
      static constexpr bool is_exact = true;
      static constexpr int digits = numeric_limits<_Sp>::digits - 1;
      static constexpr int digits10
 = static_cast<int>(digits * numbers::ln2 / numbers::ln10);

      static constexpr _Dp
      min() noexcept
      { return _Dp(_Sp(0, 1)); }

      static constexpr _Dp
      max() noexcept
      { return _Dp(_Sp(static_cast<_Sp::__rep>(-1), 0)); }

      static constexpr _Dp
      lowest() noexcept
      { return min(); }
    };


}
# 42 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ranges_base.h" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/version.h" 1 3
# 43 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ranges_base.h" 2 3

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wpedantic"
# 54 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ranges_base.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{

namespace ranges
{
  template<typename>
    inline constexpr bool disable_sized_range = false;

  template<typename _Tp>
    inline constexpr bool enable_borrowed_range = false;

  namespace __detail
  {
    constexpr __max_size_type
    __to_unsigned_like(__max_size_type __t) noexcept
    { return __t; }

    constexpr __max_size_type
    __to_unsigned_like(__max_diff_type __t) noexcept
    { return __max_size_type(__t); }

    template<integral _Tp>
      constexpr auto
      __to_unsigned_like(_Tp __t) noexcept
      { return static_cast<make_unsigned_t<_Tp>>(__t); }


    constexpr unsigned __int128
    __to_unsigned_like(__int128 __t) noexcept
    { return __t; }

    constexpr unsigned __int128
    __to_unsigned_like(unsigned __int128 __t) noexcept
    { return __t; }


    template<typename _Tp>
      using __make_unsigned_like_t
 = decltype(__detail::__to_unsigned_like(std::declval<_Tp>()));


    template<typename _Tp>
      concept __maybe_borrowed_range
 = is_lvalue_reference_v<_Tp>
   || enable_borrowed_range<remove_cvref_t<_Tp>>;

  }


  namespace __access
  {
    using std::ranges::__detail::__maybe_borrowed_range;
    using std::__detail::__range_iter_t;

    struct _Begin
    {
    private:
      template<typename _Tp>
 static constexpr bool
 _S_noexcept()
 {
   if constexpr (is_array_v<remove_reference_t<_Tp>>)
     return true;
   else if constexpr (__member_begin<_Tp>)
     return noexcept(__decay_copy(std::declval<_Tp&>().begin()));
   else
     return noexcept(__decay_copy(begin(std::declval<_Tp&>())));
 }

    public:
      template<__maybe_borrowed_range _Tp>
 requires is_array_v<remove_reference_t<_Tp>> || __member_begin<_Tp>
   || __adl_begin<_Tp>
 constexpr auto
 operator()[[nodiscard]](_Tp&& __t) const noexcept(_S_noexcept<_Tp&>())
 {
   if constexpr (is_array_v<remove_reference_t<_Tp>>)
     {
       static_assert(is_lvalue_reference_v<_Tp>);
       return __t + 0;
     }
   else if constexpr (__member_begin<_Tp>)
     return __t.begin();
   else
     return begin(__t);
 }
    };

    template<typename _Tp>
      concept __member_end = requires(_Tp& __t)
 {
   { __decay_copy(__t.end()) } -> sentinel_for<__range_iter_t<_Tp>>;
 };


    void end() = delete;

    template<typename _Tp>
      concept __adl_end = __class_or_enum<remove_reference_t<_Tp>>
 && requires(_Tp& __t)
 {
   { __decay_copy(end(__t)) } -> sentinel_for<__range_iter_t<_Tp>>;
 };

    struct _End
    {
    private:
      template<typename _Tp>
 static constexpr bool
 _S_noexcept()
 {
   if constexpr (is_bounded_array_v<remove_reference_t<_Tp>>)
     return true;
   else if constexpr (__member_end<_Tp>)
     return noexcept(__decay_copy(std::declval<_Tp&>().end()));
   else
     return noexcept(__decay_copy(end(std::declval<_Tp&>())));
 }

    public:
      template<__maybe_borrowed_range _Tp>
 requires is_bounded_array_v<remove_reference_t<_Tp>>
   || __member_end<_Tp> || __adl_end<_Tp>
 constexpr auto
 operator()[[nodiscard]](_Tp&& __t) const noexcept(_S_noexcept<_Tp&>())
 {
   if constexpr (is_bounded_array_v<remove_reference_t<_Tp>>)
     {
       static_assert(is_lvalue_reference_v<_Tp>);
       return __t + extent_v<remove_reference_t<_Tp>>;
     }
   else if constexpr (__member_end<_Tp>)
     return __t.end();
   else
     return end(__t);
 }
    };

    template<typename _Tp>
      concept __member_rbegin = requires(_Tp& __t)
 {
   { __decay_copy(__t.rbegin()) } -> input_or_output_iterator;
 };

    void rbegin() = delete;

    template<typename _Tp>
      concept __adl_rbegin = __class_or_enum<remove_reference_t<_Tp>>
 && requires(_Tp& __t)
 {
   { __decay_copy(rbegin(__t)) } -> input_or_output_iterator;
 };

    template<typename _Tp>
      concept __reversable = requires(_Tp& __t)
 {
   { _Begin{}(__t) } -> bidirectional_iterator;
   { _End{}(__t) } -> same_as<decltype(_Begin{}(__t))>;
 };

    struct _RBegin
    {
    private:
      template<typename _Tp>
 static constexpr bool
 _S_noexcept()
 {
   if constexpr (__member_rbegin<_Tp>)
     return noexcept(__decay_copy(std::declval<_Tp&>().rbegin()));
   else if constexpr (__adl_rbegin<_Tp>)
     return noexcept(__decay_copy(rbegin(std::declval<_Tp&>())));
   else
     {
       if constexpr (noexcept(_End{}(std::declval<_Tp&>())))
  {
    using _It = decltype(_End{}(std::declval<_Tp&>()));

    return is_nothrow_copy_constructible_v<_It>;
  }
       else
  return false;
     }
 }

    public:
      template<__maybe_borrowed_range _Tp>
 requires __member_rbegin<_Tp> || __adl_rbegin<_Tp> || __reversable<_Tp>
 constexpr auto
 operator()[[nodiscard]](_Tp&& __t) const
 noexcept(_S_noexcept<_Tp&>())
 {
   if constexpr (__member_rbegin<_Tp>)
     return __t.rbegin();
   else if constexpr (__adl_rbegin<_Tp>)
     return rbegin(__t);
   else
     return std::make_reverse_iterator(_End{}(__t));
 }
    };

    template<typename _Tp>
      concept __member_rend = requires(_Tp& __t)
 {
   { __decay_copy(__t.rend()) }
     -> sentinel_for<decltype(_RBegin{}(std::forward<_Tp>(__t)))>;
 };

    void rend() = delete;

    template<typename _Tp>
      concept __adl_rend = __class_or_enum<remove_reference_t<_Tp>>
 && requires(_Tp& __t)
 {
   { __decay_copy(rend(__t)) }
     -> sentinel_for<decltype(_RBegin{}(std::forward<_Tp>(__t)))>;
 };

    struct _REnd
    {
    private:
      template<typename _Tp>
 static constexpr bool
 _S_noexcept()
 {
   if constexpr (__member_rend<_Tp>)
     return noexcept(__decay_copy(std::declval<_Tp&>().rend()));
   else if constexpr (__adl_rend<_Tp>)
     return noexcept(__decay_copy(rend(std::declval<_Tp&>())));
   else
     {
       if constexpr (noexcept(_Begin{}(std::declval<_Tp&>())))
  {
    using _It = decltype(_Begin{}(std::declval<_Tp&>()));

    return is_nothrow_copy_constructible_v<_It>;
  }
       else
  return false;
     }
 }

    public:
      template<__maybe_borrowed_range _Tp>
 requires __member_rend<_Tp> || __adl_rend<_Tp> || __reversable<_Tp>
 constexpr auto
 operator()[[nodiscard]](_Tp&& __t) const
 noexcept(_S_noexcept<_Tp&>())
 {
   if constexpr (__member_rend<_Tp>)
     return __t.rend();
   else if constexpr (__adl_rend<_Tp>)
     return rend(__t);
   else
     return std::make_reverse_iterator(_Begin{}(__t));
 }
    };

    template<typename _Tp>
      concept __member_size = !disable_sized_range<remove_cvref_t<_Tp>>
 && requires(_Tp& __t)
 {
   { __decay_copy(__t.size()) } -> __detail::__is_integer_like;
 };

    void size() = delete;

    template<typename _Tp>
      concept __adl_size = __class_or_enum<remove_reference_t<_Tp>>
 && !disable_sized_range<remove_cvref_t<_Tp>>
 && requires(_Tp& __t)
 {
   { __decay_copy(size(__t)) } -> __detail::__is_integer_like;
 };

    template<typename _Tp>
      concept __sentinel_size = requires(_Tp& __t)
 {
   requires (!is_unbounded_array_v<remove_reference_t<_Tp>>);

   { _Begin{}(__t) } -> forward_iterator;

   { _End{}(__t) } -> sized_sentinel_for<decltype(_Begin{}(__t))>;

   __detail::__to_unsigned_like(_End{}(__t) - _Begin{}(__t));
 };

    struct _Size
    {
    private:
      template<typename _Tp>
 static constexpr bool
 _S_noexcept()
 {
   if constexpr (is_bounded_array_v<remove_reference_t<_Tp>>)
     return true;
   else if constexpr (__member_size<_Tp>)
     return noexcept(__decay_copy(std::declval<_Tp&>().size()));
   else if constexpr (__adl_size<_Tp>)
     return noexcept(__decay_copy(size(std::declval<_Tp&>())));
   else if constexpr (__sentinel_size<_Tp>)
     return noexcept(_End{}(std::declval<_Tp&>())
       - _Begin{}(std::declval<_Tp&>()));
 }

    public:
      template<typename _Tp>
 requires is_bounded_array_v<remove_reference_t<_Tp>>
   || __member_size<_Tp> || __adl_size<_Tp> || __sentinel_size<_Tp>
 constexpr auto
 operator()[[nodiscard]](_Tp&& __t) const noexcept(_S_noexcept<_Tp&>())
 {
   if constexpr (is_bounded_array_v<remove_reference_t<_Tp>>)
     return extent_v<remove_reference_t<_Tp>>;
   else if constexpr (__member_size<_Tp>)
     return __t.size();
   else if constexpr (__adl_size<_Tp>)
     return size(__t);
   else if constexpr (__sentinel_size<_Tp>)
     return __detail::__to_unsigned_like(_End{}(__t) - _Begin{}(__t));
 }
    };

    struct _SSize
    {


      template<typename _Tp>
 requires requires (_Tp& __t) { _Size{}(__t); }
 constexpr auto
 operator()[[nodiscard]](_Tp&& __t) const noexcept(noexcept(_Size{}(__t)))
 {
   auto __size = _Size{}(__t);
   using __size_type = decltype(__size);

   if constexpr (integral<__size_type>)
     {
       using __gnu_cxx::__int_traits;
       if constexpr (__int_traits<__size_type>::__digits
       < __int_traits<ptrdiff_t>::__digits)
  return static_cast<ptrdiff_t>(__size);
       else
  return static_cast<make_signed_t<__size_type>>(__size);
     }


   else if constexpr (__detail::__is_int128<__size_type>)
     return static_cast<__int128>(__size);

   else
     return __detail::__max_diff_type(__size);
 }
    };

    template<typename _Tp>
      concept __member_empty = requires(_Tp& __t) { bool(__t.empty()); };

    template<typename _Tp>
      concept __size0_empty = requires(_Tp& __t) { _Size{}(__t) == 0; };

    template<typename _Tp>
      concept __eq_iter_empty = requires(_Tp& __t)
 {
   requires (!is_unbounded_array_v<remove_reference_t<_Tp>>);

   { _Begin{}(__t) } -> forward_iterator;

   bool(_Begin{}(__t) == _End{}(__t));
 };

    struct _Empty
    {
    private:
      template<typename _Tp>
 static constexpr bool
 _S_noexcept()
 {
   if constexpr (__member_empty<_Tp>)
     return noexcept(bool(std::declval<_Tp&>().empty()));
   else if constexpr (__size0_empty<_Tp>)
     return noexcept(_Size{}(std::declval<_Tp&>()) == 0);
   else
     return noexcept(bool(_Begin{}(std::declval<_Tp&>())
  == _End{}(std::declval<_Tp&>())));
 }

    public:
      template<typename _Tp>
 requires __member_empty<_Tp> || __size0_empty<_Tp>
   || __eq_iter_empty<_Tp>
 constexpr bool
 operator()[[nodiscard]](_Tp&& __t) const noexcept(_S_noexcept<_Tp&>())
 {
   if constexpr (__member_empty<_Tp>)
     return bool(__t.empty());
   else if constexpr (__size0_empty<_Tp>)
     return _Size{}(__t) == 0;
   else
     return bool(_Begin{}(__t) == _End{}(__t));
 }
    };

    template<typename _Tp>
      concept __pointer_to_object = is_pointer_v<_Tp>
        && is_object_v<remove_pointer_t<_Tp>>;

    template<typename _Tp>
      concept __member_data = requires(_Tp& __t)
 {
   { __decay_copy(__t.data()) } -> __pointer_to_object;
 };

    template<typename _Tp>
      concept __begin_data = contiguous_iterator<__range_iter_t<_Tp>>;

    struct _Data
    {
    private:
      template<typename _Tp>
 static constexpr bool
 _S_noexcept()
 {
   if constexpr (__member_data<_Tp>)
     return noexcept(__decay_copy(std::declval<_Tp&>().data()));
   else
     return noexcept(_Begin{}(std::declval<_Tp&>()));
 }

    public:
      template<__maybe_borrowed_range _Tp>
 requires __member_data<_Tp> || __begin_data<_Tp>
 constexpr auto
 operator()[[nodiscard]](_Tp&& __t) const noexcept(_S_noexcept<_Tp>())
 {
   if constexpr (__member_data<_Tp>)
     return __t.data();
   else
     return std::to_address(_Begin{}(__t));
 }
    };

  }

  inline namespace _Cpo
  {
    inline constexpr ranges::__access::_Begin begin{};
    inline constexpr ranges::__access::_End end{};
    inline constexpr ranges::__access::_RBegin rbegin{};
    inline constexpr ranges::__access::_REnd rend{};
    inline constexpr ranges::__access::_Size size{};
    inline constexpr ranges::__access::_SSize ssize{};
    inline constexpr ranges::__access::_Empty empty{};
    inline constexpr ranges::__access::_Data data{};
  }


  template<typename _Tp>
    concept range = requires(_Tp& __t)
      {
 ranges::begin(__t);
 ranges::end(__t);
      };


  template<typename _Tp>
    concept borrowed_range
      = range<_Tp> && __detail::__maybe_borrowed_range<_Tp>;

  template<typename _Tp>
    using iterator_t = std::__detail::__range_iter_t<_Tp>;

  template<range _Range>
    using sentinel_t = decltype(ranges::end(std::declval<_Range&>()));
# 538 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ranges_base.h" 3
  template<range _Range>
    using range_difference_t = iter_difference_t<iterator_t<_Range>>;

  template<range _Range>
    using range_value_t = iter_value_t<iterator_t<_Range>>;

  template<range _Range>
    using range_reference_t = iter_reference_t<iterator_t<_Range>>;

  template<range _Range>
    using range_rvalue_reference_t
      = iter_rvalue_reference_t<iterator_t<_Range>>;



  template<range _Range>
    using range_common_reference_t
      = iter_common_reference_t<iterator_t<_Range>>;


  template<typename _Tp>
    concept sized_range = range<_Tp>
      && requires(_Tp& __t) { ranges::size(__t); };

  template<sized_range _Range>
    using range_size_t = decltype(ranges::size(std::declval<_Range&>()));

  template<typename _Derived>
    requires is_class_v<_Derived> && same_as<_Derived, remove_cv_t<_Derived>>
    class view_interface;

  namespace __detail
  {
    template<typename _Tp, typename _Up>
      requires (!same_as<_Tp, view_interface<_Up>>)
      void __is_derived_from_view_interface_fn(const _Tp&,
            const view_interface<_Up>&);



    template<typename _Tp>
      concept __is_derived_from_view_interface
 = requires (_Tp __t) { __is_derived_from_view_interface_fn(__t, __t); };
  }


  struct view_base { };


  template<typename _Tp>
    inline constexpr bool enable_view = derived_from<_Tp, view_base>
      || __detail::__is_derived_from_view_interface<_Tp>;


  template<typename _Tp>
    concept view
      = range<_Tp> && movable<_Tp> && enable_view<_Tp>;




  template<typename _Range, typename _Tp>
    concept output_range
      = range<_Range> && output_iterator<iterator_t<_Range>, _Tp>;


  template<typename _Tp>
    concept input_range = range<_Tp> && input_iterator<iterator_t<_Tp>>;


  template<typename _Tp>
    concept forward_range
      = input_range<_Tp> && forward_iterator<iterator_t<_Tp>>;


  template<typename _Tp>
    concept bidirectional_range
      = forward_range<_Tp> && bidirectional_iterator<iterator_t<_Tp>>;


  template<typename _Tp>
    concept random_access_range
      = bidirectional_range<_Tp> && random_access_iterator<iterator_t<_Tp>>;


  template<typename _Tp>
    concept contiguous_range
      = random_access_range<_Tp> && contiguous_iterator<iterator_t<_Tp>>
      && requires(_Tp& __t)
      {
 { ranges::data(__t) } -> same_as<add_pointer_t<range_reference_t<_Tp>>>;
      };


  template<typename _Tp>
    concept common_range
      = range<_Tp> && same_as<iterator_t<_Tp>, sentinel_t<_Tp>>;







  namespace __access
  {
# 656 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ranges_base.h" 3
    template<typename _To, typename _Tp>
      constexpr decltype(auto)
      __as_const(_Tp& __t) noexcept
      {
 static_assert(std::is_same_v<_To&, _Tp&>);

 if constexpr (is_lvalue_reference_v<_To>)
   return const_cast<const _Tp&>(__t);
 else
   return static_cast<const _Tp&&>(__t);
      }


    struct _CBegin
    {
# 685 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ranges_base.h" 3
      template<typename _Tp>
 [[nodiscard]]
 constexpr auto
 operator()(_Tp&& __e) const
 noexcept(noexcept(_Begin{}(__access::__as_const<_Tp>(__e))))
 requires requires { _Begin{}(__access::__as_const<_Tp>(__e)); }
 {
   return _Begin{}(__access::__as_const<_Tp>(__e));
 }

    };

    struct _CEnd final
    {
# 713 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ranges_base.h" 3
      template<typename _Tp>
 [[nodiscard]]
 constexpr auto
 operator()(_Tp&& __e) const
 noexcept(noexcept(_End{}(__access::__as_const<_Tp>(__e))))
 requires requires { _End{}(__access::__as_const<_Tp>(__e)); }
 {
   return _End{}(__access::__as_const<_Tp>(__e));
 }

    };

    struct _CRBegin
    {
# 741 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ranges_base.h" 3
      template<typename _Tp>
 [[nodiscard]]
 constexpr auto
 operator()(_Tp&& __e) const
 noexcept(noexcept(_RBegin{}(__access::__as_const<_Tp>(__e))))
 requires requires { _RBegin{}(__access::__as_const<_Tp>(__e)); }
 {
   return _RBegin{}(__access::__as_const<_Tp>(__e));
 }

    };

    struct _CREnd
    {
# 769 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ranges_base.h" 3
      template<typename _Tp>
 [[nodiscard]]
 constexpr auto
 operator()(_Tp&& __e) const
 noexcept(noexcept(_REnd{}(__access::__as_const<_Tp>(__e))))
 requires requires { _REnd{}(__access::__as_const<_Tp>(__e)); }
 {
   return _REnd{}(__access::__as_const<_Tp>(__e));
 }

    };

    struct _CData
    {
# 792 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ranges_base.h" 3
      template<typename _Tp>
 [[nodiscard]]
 constexpr auto
 operator()(_Tp&& __e) const
 noexcept(noexcept(_Data{}(__access::__as_const<_Tp>(__e))))
 requires requires { _Data{}(__access::__as_const<_Tp>(__e)); }
 {
   return _Data{}(__access::__as_const<_Tp>(__e));
 }

    };
  }

  inline namespace _Cpo
  {
    inline constexpr ranges::__access::_CBegin cbegin{};
    inline constexpr ranges::__access::_CEnd cend{};
    inline constexpr ranges::__access::_CRBegin crbegin{};
    inline constexpr ranges::__access::_CREnd crend{};
    inline constexpr ranges::__access::_CData cdata{};
  }

  namespace __detail
  {
    template<typename _Tp>
      inline constexpr bool __is_initializer_list = false;

    template<typename _Tp>
      inline constexpr bool __is_initializer_list<initializer_list<_Tp>> = true;
  }


  template<typename _Tp>
    concept viewable_range = range<_Tp>
      && ((view<remove_cvref_t<_Tp>> && constructible_from<remove_cvref_t<_Tp>, _Tp>)
   || (!view<remove_cvref_t<_Tp>>
       && (is_lvalue_reference_v<_Tp>
    || (movable<remove_reference_t<_Tp>>
        && !__detail::__is_initializer_list<remove_cvref_t<_Tp>>))));



  struct __advance_fn final
  {
    template<input_or_output_iterator _It>
      constexpr void
      operator()(_It& __it, iter_difference_t<_It> __n) const
      {
 if constexpr (random_access_iterator<_It>)
   __it += __n;
 else if constexpr (bidirectional_iterator<_It>)
   {
     if (__n > 0)
       {
  do
    {
      ++__it;
    }
  while (--__n);
       }
     else if (__n < 0)
       {
  do
    {
      --__it;
    }
  while (++__n);
       }
   }
 else
   {

     do { if (__builtin_expect(!bool(__n >= 0), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ranges_base.h", 864, __PRETTY_FUNCTION__, "__n >= 0"); } while (false);
     while (__n-- > 0)
       ++__it;
   }
      }

    template<input_or_output_iterator _It, sentinel_for<_It> _Sent>
      constexpr void
      operator()(_It& __it, _Sent __bound) const
      {
 if constexpr (assignable_from<_It&, _Sent>)
   __it = std::move(__bound);
 else if constexpr (sized_sentinel_for<_Sent, _It>)
   (*this)(__it, __bound - __it);
 else
   {
     while (__it != __bound)
       ++__it;
   }
      }

    template<input_or_output_iterator _It, sentinel_for<_It> _Sent>
      constexpr iter_difference_t<_It>
      operator()(_It& __it, iter_difference_t<_It> __n, _Sent __bound) const
      {
 if constexpr (sized_sentinel_for<_Sent, _It>)
   {
     const auto __diff = __bound - __it;

     if (__diff == 0)
       return __n;
     else if (__diff > 0 ? __n >= __diff : __n <= __diff)
       {
  (*this)(__it, __bound);
  return __n - __diff;
       }
     else if (__n != 0) [[likely]]
       {

  do { if (__builtin_expect(!bool((__n < 0) == (__diff < 0)), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ranges_base.h", 903, __PRETTY_FUNCTION__, "(__n < 0) == (__diff < 0)"); } while (false);

  (*this)(__it, __n);
  return 0;
       }
     else
       return 0;
   }
 else if (__it == __bound || __n == 0)
   return __n;
 else if (__n > 0)
   {
     iter_difference_t<_It> __m = 0;
     do
       {
  ++__it;
  ++__m;
       }
     while (__m != __n && __it != __bound);
     return __n - __m;
   }
 else if constexpr (bidirectional_iterator<_It> && same_as<_It, _Sent>)
   {
     iter_difference_t<_It> __m = 0;
     do
       {
  --__it;
  --__m;
       }
     while (__m != __n && __it != __bound);
     return __n - __m;
   }
 else
   {

     do { if (__builtin_expect(!bool(__n >= 0), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ranges_base.h", 938, __PRETTY_FUNCTION__, "__n >= 0"); } while (false);
     return __n;
   }
      }

    void operator&() const = delete;
  };

  inline constexpr __advance_fn advance{};

  struct __distance_fn final
  {


    template<typename _It, sentinel_for<_It> _Sent>
      requires (!sized_sentinel_for<_Sent, _It>)
      constexpr iter_difference_t<_It>
      operator()[[nodiscard]](_It __first, _Sent __last) const
      {
 iter_difference_t<_It> __n = 0;
 while (__first != __last)
   {
     ++__first;
     ++__n;
   }
 return __n;
      }

    template<typename _It, sized_sentinel_for<decay_t<_It>> _Sent>
      [[nodiscard]]
      constexpr iter_difference_t<decay_t<_It>>
      operator()(_It&& __first, _Sent __last) const
      { return __last - static_cast<const decay_t<_It>&>(__first); }

    template<range _Range>
      [[nodiscard]]
      constexpr range_difference_t<_Range>
      operator()(_Range&& __r) const
      {
 if constexpr (sized_range<_Range>)
   return static_cast<range_difference_t<_Range>>(ranges::size(__r));
 else
   return (*this)(ranges::begin(__r), ranges::end(__r));
      }

    void operator&() const = delete;
  };

  inline constexpr __distance_fn distance{};

  struct __next_fn final
  {
    template<input_or_output_iterator _It>
      [[nodiscard]]
      constexpr _It
      operator()(_It __x) const
      {
 ++__x;
 return __x;
      }

    template<input_or_output_iterator _It>
      [[nodiscard]]
      constexpr _It
      operator()(_It __x, iter_difference_t<_It> __n) const
      {
 ranges::advance(__x, __n);
 return __x;
      }

    template<input_or_output_iterator _It, sentinel_for<_It> _Sent>
      [[nodiscard]]
      constexpr _It
      operator()(_It __x, _Sent __bound) const
      {
 ranges::advance(__x, __bound);
 return __x;
      }

    template<input_or_output_iterator _It, sentinel_for<_It> _Sent>
      [[nodiscard]]
      constexpr _It
      operator()(_It __x, iter_difference_t<_It> __n, _Sent __bound) const
      {
 ranges::advance(__x, __n, __bound);
 return __x;
      }

    void operator&() const = delete;
  };

  inline constexpr __next_fn next{};

  struct __prev_fn final
  {
    template<bidirectional_iterator _It>
      [[nodiscard]]
      constexpr _It
      operator()(_It __x) const
      {
 --__x;
 return __x;
      }

    template<bidirectional_iterator _It>
      [[nodiscard]]
      constexpr _It
      operator()(_It __x, iter_difference_t<_It> __n) const
      {
 ranges::advance(__x, -__n);
 return __x;
      }

    template<bidirectional_iterator _It>
      [[nodiscard]]
      constexpr _It
      operator()(_It __x, iter_difference_t<_It> __n, _It __bound) const
      {
 ranges::advance(__x, -__n, __bound);
 return __x;
      }

    void operator&() const = delete;
  };

  inline constexpr __prev_fn prev{};


  struct dangling
  {
    constexpr dangling() noexcept = default;
    template<typename... _Args>
      constexpr dangling(_Args&&...) noexcept { }
  };

  template<range _Range>
    using borrowed_iterator_t = __conditional_t<borrowed_range<_Range>,
      iterator_t<_Range>,
      dangling>;
}
# 1102 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ranges_base.h" 3

}

#pragma GCC diagnostic pop
# 35 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ranges_util.h" 2 3





namespace std __attribute__ ((__visibility__ ("default")))
{

namespace ranges
{


  namespace __detail
  {
    template<typename _Range>
      concept __simple_view = view<_Range> && range<const _Range>
 && same_as<iterator_t<_Range>, iterator_t<const _Range>>
 && same_as<sentinel_t<_Range>, sentinel_t<const _Range>>;

    template<typename _It>
      concept __has_arrow = input_iterator<_It>
 && (is_pointer_v<_It> || requires(_It __it) { __it.operator->(); });

    using std::__detail::__different_from;
  }


  template<typename _Derived>
    requires is_class_v<_Derived> && same_as<_Derived, remove_cv_t<_Derived>>
    class view_interface
    {
    private:
      constexpr _Derived& _M_derived() noexcept
      {
 static_assert(derived_from<_Derived, view_interface<_Derived>>);
 static_assert(view<_Derived>);
 return static_cast<_Derived&>(*this);
      }

      constexpr const _Derived& _M_derived() const noexcept
      {
 static_assert(derived_from<_Derived, view_interface<_Derived>>);
 static_assert(view<_Derived>);
 return static_cast<const _Derived&>(*this);
      }

      static constexpr bool
      _S_bool(bool) noexcept;

      template<typename _Tp>
 static constexpr bool
 _S_empty(_Tp& __t)
 noexcept(noexcept(_S_bool(ranges::begin(__t) == ranges::end(__t))))
 { return ranges::begin(__t) == ranges::end(__t); }

      template<typename _Tp>
 static constexpr auto
 _S_size(_Tp& __t)
 noexcept(noexcept(ranges::end(__t) - ranges::begin(__t)))
 { return ranges::end(__t) - ranges::begin(__t); }

    public:
      constexpr bool
      empty()
      noexcept(noexcept(_S_empty(_M_derived())))
      requires forward_range<_Derived> && (!sized_range<_Derived>)
      { return _S_empty(_M_derived()); }

      constexpr bool
      empty()
      noexcept(noexcept(ranges::size(_M_derived()) == 0))
      requires sized_range<_Derived>
      { return ranges::size(_M_derived()) == 0; }

      constexpr bool
      empty() const
      noexcept(noexcept(_S_empty(_M_derived())))
      requires forward_range<const _Derived> && (!sized_range<const _Derived>)
      { return _S_empty(_M_derived()); }

      constexpr bool
      empty() const
      noexcept(noexcept(ranges::size(_M_derived()) == 0))
      requires sized_range<const _Derived>
      { return ranges::size(_M_derived()) == 0; }

      constexpr explicit
      operator bool() noexcept(noexcept(ranges::empty(_M_derived())))
      requires requires { ranges::empty(_M_derived()); }
      { return !ranges::empty(_M_derived()); }

      constexpr explicit
      operator bool() const noexcept(noexcept(ranges::empty(_M_derived())))
      requires requires { ranges::empty(_M_derived()); }
      { return !ranges::empty(_M_derived()); }

      constexpr auto
      data() noexcept(noexcept(ranges::begin(_M_derived())))
      requires contiguous_iterator<iterator_t<_Derived>>
      { return std::to_address(ranges::begin(_M_derived())); }

      constexpr auto
      data() const noexcept(noexcept(ranges::begin(_M_derived())))
      requires range<const _Derived>
 && contiguous_iterator<iterator_t<const _Derived>>
      { return std::to_address(ranges::begin(_M_derived())); }

      constexpr auto
      size() noexcept(noexcept(_S_size(_M_derived())))
      requires forward_range<_Derived>
 && sized_sentinel_for<sentinel_t<_Derived>, iterator_t<_Derived>>
      { return _S_size(_M_derived()); }

      constexpr auto
      size() const noexcept(noexcept(_S_size(_M_derived())))
      requires forward_range<const _Derived>
 && sized_sentinel_for<sentinel_t<const _Derived>,
         iterator_t<const _Derived>>
      { return _S_size(_M_derived()); }

      constexpr decltype(auto)
      front() requires forward_range<_Derived>
      {
 do { if (__builtin_expect(!bool(!empty()), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ranges_util.h", 158, __PRETTY_FUNCTION__, "!empty()"); } while (false);
 return *ranges::begin(_M_derived());
      }

      constexpr decltype(auto)
      front() const requires forward_range<const _Derived>
      {
 do { if (__builtin_expect(!bool(!empty()), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ranges_util.h", 165, __PRETTY_FUNCTION__, "!empty()"); } while (false);
 return *ranges::begin(_M_derived());
      }

      constexpr decltype(auto)
      back()
      requires bidirectional_range<_Derived> && common_range<_Derived>
      {
 do { if (__builtin_expect(!bool(!empty()), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ranges_util.h", 173, __PRETTY_FUNCTION__, "!empty()"); } while (false);
 return *ranges::prev(ranges::end(_M_derived()));
      }

      constexpr decltype(auto)
      back() const
      requires bidirectional_range<const _Derived>
 && common_range<const _Derived>
      {
 do { if (__builtin_expect(!bool(!empty()), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ranges_util.h", 182, __PRETTY_FUNCTION__, "!empty()"); } while (false);
 return *ranges::prev(ranges::end(_M_derived()));
      }

      template<random_access_range _Range = _Derived>
 constexpr decltype(auto)
 operator[](range_difference_t<_Range> __n)
 { return ranges::begin(_M_derived())[__n]; }

      template<random_access_range _Range = const _Derived>
 constexpr decltype(auto)
 operator[](range_difference_t<_Range> __n) const
 { return ranges::begin(_M_derived())[__n]; }
# 213 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ranges_util.h" 3
    };

  namespace __detail
  {
    template<typename _From, typename _To>
      concept __uses_nonqualification_pointer_conversion
 = is_pointer_v<_From> && is_pointer_v<_To>
   && !convertible_to<remove_pointer_t<_From>(*)[],
        remove_pointer_t<_To>(*)[]>;

    template<typename _From, typename _To>
      concept __convertible_to_non_slicing = convertible_to<_From, _To>
 && !__uses_nonqualification_pointer_conversion<decay_t<_From>,
             decay_t<_To>>;





    template<typename _Tp>
      concept __pair_like
 = !is_reference_v<_Tp> && requires(_Tp __t)
 {
   typename tuple_size<_Tp>::type;
   requires derived_from<tuple_size<_Tp>, integral_constant<size_t, 2>>;
   typename tuple_element_t<0, remove_const_t<_Tp>>;
   typename tuple_element_t<1, remove_const_t<_Tp>>;
   { get<0>(__t) } -> convertible_to<const tuple_element_t<0, _Tp>&>;
   { get<1>(__t) } -> convertible_to<const tuple_element_t<1, _Tp>&>;
 };


    template<typename _Tp, typename _Up, typename _Vp>
      concept __pair_like_convertible_from
 = !range<_Tp> && !is_reference_v<_Vp> && __pair_like<_Tp>
 && constructible_from<_Tp, _Up, _Vp>
 && __convertible_to_non_slicing<_Up, tuple_element_t<0, _Tp>>
 && convertible_to<_Vp, tuple_element_t<1, _Tp>>;

  }

  namespace views { struct _Drop; }

  enum class subrange_kind : bool { unsized, sized };


  template<input_or_output_iterator _It, sentinel_for<_It> _Sent = _It,
    subrange_kind _Kind = sized_sentinel_for<_Sent, _It>
      ? subrange_kind::sized : subrange_kind::unsized>
    requires (_Kind == subrange_kind::sized || !sized_sentinel_for<_Sent, _It>)
    class subrange : public view_interface<subrange<_It, _Sent, _Kind>>
    {
    private:
      static constexpr bool _S_store_size
 = _Kind == subrange_kind::sized && !sized_sentinel_for<_Sent, _It>;

      friend struct views::_Drop;

      _It _M_begin = _It();
      [[no_unique_address]] _Sent _M_end = _Sent();

      using __size_type
 = __detail::__make_unsigned_like_t<iter_difference_t<_It>>;

      template<typename _Tp, bool = _S_store_size>
 struct _Size
 {
   [[__gnu__::__always_inline__]]
   constexpr _Size(_Tp = {}) { }
 };

      template<typename _Tp>
 struct _Size<_Tp, true>
 {
   [[__gnu__::__always_inline__]]
   constexpr _Size(_Tp __s = {}) : _M_size(__s) { }

   _Tp _M_size;
 };

      [[no_unique_address]] _Size<__size_type> _M_size = {};

    public:
      subrange() requires default_initializable<_It> = default;

      constexpr
      subrange(__detail::__convertible_to_non_slicing<_It> auto __i, _Sent __s)
      noexcept(is_nothrow_constructible_v<_It, decltype(__i)>
        && is_nothrow_constructible_v<_Sent, _Sent&>)
 requires (!_S_store_size)
      : _M_begin(std::move(__i)), _M_end(__s)
      { }

      constexpr
      subrange(__detail::__convertible_to_non_slicing<_It> auto __i, _Sent __s,
        __size_type __n)
      noexcept(is_nothrow_constructible_v<_It, decltype(__i)>
        && is_nothrow_constructible_v<_Sent, _Sent&>)
 requires (_Kind == subrange_kind::sized)
      : _M_begin(std::move(__i)), _M_end(__s), _M_size(__n)
      { }

      template<__detail::__different_from<subrange> _Rng>
 requires borrowed_range<_Rng>
   && __detail::__convertible_to_non_slicing<iterator_t<_Rng>, _It>
   && convertible_to<sentinel_t<_Rng>, _Sent>
 constexpr
 subrange(_Rng&& __r)
 noexcept(noexcept(subrange(__r, ranges::size(__r))))
 requires _S_store_size && sized_range<_Rng>
 : subrange(__r, ranges::size(__r))
 { }

      template<__detail::__different_from<subrange> _Rng>
 requires borrowed_range<_Rng>
   && __detail::__convertible_to_non_slicing<iterator_t<_Rng>, _It>
   && convertible_to<sentinel_t<_Rng>, _Sent>
 constexpr
 subrange(_Rng&& __r)
 noexcept(noexcept(subrange(ranges::begin(__r), ranges::end(__r))))
 requires (!_S_store_size)
 : subrange(ranges::begin(__r), ranges::end(__r))
 { }

      template<borrowed_range _Rng>
 requires __detail::__convertible_to_non_slicing<iterator_t<_Rng>, _It>
   && convertible_to<sentinel_t<_Rng>, _Sent>
 constexpr
 subrange(_Rng&& __r, __size_type __n)
 noexcept(noexcept(subrange(ranges::begin(__r), ranges::end(__r), __n)))
 requires (_Kind == subrange_kind::sized)
 : subrange{ranges::begin(__r), ranges::end(__r), __n}
 { }

      template<__detail::__different_from<subrange> _PairLike>
 requires __detail::__pair_like_convertible_from<_PairLike, const _It&,
       const _Sent&>
 constexpr
 operator _PairLike() const
 { return _PairLike(_M_begin, _M_end); }

      constexpr _It
      begin() const requires copyable<_It>
      { return _M_begin; }

      [[nodiscard]] constexpr _It
      begin() requires (!copyable<_It>)
      { return std::move(_M_begin); }

      constexpr _Sent end() const { return _M_end; }

      constexpr bool empty() const { return _M_begin == _M_end; }

      constexpr __size_type
      size() const requires (_Kind == subrange_kind::sized)
      {
 if constexpr (_S_store_size)
   return _M_size._M_size;
 else
   return __detail::__to_unsigned_like(_M_end - _M_begin);
      }

      [[nodiscard]] constexpr subrange
      next(iter_difference_t<_It> __n = 1) const &
 requires forward_iterator<_It>
      {
 auto __tmp = *this;
 __tmp.advance(__n);
 return __tmp;
      }

      [[nodiscard]] constexpr subrange
      next(iter_difference_t<_It> __n = 1) &&
      {
 advance(__n);
 return std::move(*this);
      }

      [[nodiscard]] constexpr subrange
      prev(iter_difference_t<_It> __n = 1) const
 requires bidirectional_iterator<_It>
      {
 auto __tmp = *this;
 __tmp.advance(-__n);
 return __tmp;
      }

      constexpr subrange&
      advance(iter_difference_t<_It> __n)
      {


 if constexpr (bidirectional_iterator<_It>)
   if (__n < 0)
     {
       ranges::advance(_M_begin, __n);
       if constexpr (_S_store_size)
  _M_size._M_size += __detail::__to_unsigned_like(-__n);
       return *this;
     }

 do { if (__builtin_expect(!bool(__n >= 0), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ranges_util.h", 414, __PRETTY_FUNCTION__, "__n >= 0"); } while (false);
 auto __d = __n - ranges::advance(_M_begin, __n, _M_end);
 if constexpr (_S_store_size)
   _M_size._M_size -= __detail::__to_unsigned_like(__d);
 return *this;
      }
    };

  template<input_or_output_iterator _It, sentinel_for<_It> _Sent>
    subrange(_It, _Sent) -> subrange<_It, _Sent>;

  template<input_or_output_iterator _It, sentinel_for<_It> _Sent>
    subrange(_It, _Sent,
      __detail::__make_unsigned_like_t<iter_difference_t<_It>>)
      -> subrange<_It, _Sent, subrange_kind::sized>;

  template<borrowed_range _Rng>
    subrange(_Rng&&)
      -> subrange<iterator_t<_Rng>, sentinel_t<_Rng>,
   (sized_range<_Rng>
    || sized_sentinel_for<sentinel_t<_Rng>, iterator_t<_Rng>>)
   ? subrange_kind::sized : subrange_kind::unsized>;

  template<borrowed_range _Rng>
    subrange(_Rng&&,
      __detail::__make_unsigned_like_t<range_difference_t<_Rng>>)
      -> subrange<iterator_t<_Rng>, sentinel_t<_Rng>, subrange_kind::sized>;




  template<size_t _Num, class _It, class _Sent, subrange_kind _Kind>
    requires ((_Num == 0 && copyable<_It>) || _Num == 1)
    constexpr auto
    get(const subrange<_It, _Sent, _Kind>& __r)
    {
      if constexpr (_Num == 0)
 return __r.begin();
      else
 return __r.end();
    }

  template<size_t _Num, class _It, class _Sent, subrange_kind _Kind>
    requires (_Num < 2)
    constexpr auto
    get(subrange<_It, _Sent, _Kind>&& __r)
    {
      if constexpr (_Num == 0)
 return __r.begin();
      else
 return __r.end();
    }

  template<typename _It, typename _Sent, subrange_kind _Kind>
    inline constexpr bool
      enable_borrowed_range<subrange<_It, _Sent, _Kind>> = true;

  template<range _Range>
    using borrowed_subrange_t = __conditional_t<borrowed_range<_Range>,
      subrange<iterator_t<_Range>>,
      dangling>;


  template<typename _Iter, typename _Sent, subrange_kind _Kind>
    inline constexpr bool __detail::__is_subrange<subrange<_Iter, _Sent, _Kind>> = true;
}
# 489 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ranges_util.h" 3
namespace ranges
{
  struct __find_fn
  {
    template<input_iterator _Iter, sentinel_for<_Iter> _Sent,
      typename _Proj = identity,
      typename _Tp >
      requires indirect_binary_predicate<ranges::equal_to,
      projected<_Iter, _Proj>, const _Tp*>
      constexpr _Iter
      operator()(_Iter __first, _Sent __last,
   const _Tp& __value, _Proj __proj = {}) const
      {
 if constexpr (is_same_v<_Proj, identity>)
   if constexpr(__can_use_memchr_for_find<iter_value_t<_Iter>, _Tp>)
     if constexpr (sized_sentinel_for<_Sent, _Iter>)
       if constexpr (contiguous_iterator<_Iter>)
  if (!is_constant_evaluated())
    {
      using _Vt = iter_value_t<_Iter>;
      auto __n = __last - __first;
      if (static_cast<_Vt>(__value) == __value) [[likely]]
        if (__n > 0)
   {
     const size_t __nu = static_cast<size_t>(__n);
     const int __ival = static_cast<int>(__value);
     const void* __p0 = std::to_address(__first);
     if (auto __p1 = __builtin_memchr(__p0, __ival, __nu))
       __n = (const char*)__p1 - (const char*)__p0;
   }
      return __first + __n;
    }

 while (__first != __last
     && !(std::__invoke(__proj, *__first) == __value))
   ++__first;
 return __first;
      }

    template<input_range _Range, typename _Proj = identity,
      typename _Tp
      >
      requires indirect_binary_predicate<ranges::equal_to,
      projected<iterator_t<_Range>, _Proj>,
      const _Tp*>
      constexpr borrowed_iterator_t<_Range>
      operator()(_Range&& __r, const _Tp& __value, _Proj __proj = {}) const
      {
 return (*this)(ranges::begin(__r), ranges::end(__r),
         __value, std::move(__proj));
      }
  };

  inline constexpr __find_fn find{};

  struct __find_if_fn
  {
    template<input_iterator _Iter, sentinel_for<_Iter> _Sent,
      typename _Proj = identity,
      indirect_unary_predicate<projected<_Iter, _Proj>> _Pred>
      constexpr _Iter
      operator()(_Iter __first, _Sent __last,
   _Pred __pred, _Proj __proj = {}) const
      {
 while (__first != __last
     && !(bool)std::__invoke(__pred, std::__invoke(__proj, *__first)))
   ++__first;
 return __first;
      }

    template<input_range _Range, typename _Proj = identity,
      indirect_unary_predicate<projected<iterator_t<_Range>, _Proj>>
        _Pred>
      constexpr borrowed_iterator_t<_Range>
      operator()(_Range&& __r, _Pred __pred, _Proj __proj = {}) const
      {
 return (*this)(ranges::begin(__r), ranges::end(__r),
         std::move(__pred), std::move(__proj));
      }
  };

  inline constexpr __find_if_fn find_if{};

  struct __find_if_not_fn
  {
    template<input_iterator _Iter, sentinel_for<_Iter> _Sent,
      typename _Proj = identity,
      indirect_unary_predicate<projected<_Iter, _Proj>> _Pred>
      constexpr _Iter
      operator()(_Iter __first, _Sent __last,
   _Pred __pred, _Proj __proj = {}) const
      {
 while (__first != __last
     && (bool)std::__invoke(__pred, std::__invoke(__proj, *__first)))
   ++__first;
 return __first;
      }

    template<input_range _Range, typename _Proj = identity,
      indirect_unary_predicate<projected<iterator_t<_Range>, _Proj>>
        _Pred>
      constexpr borrowed_iterator_t<_Range>
      operator()(_Range&& __r, _Pred __pred, _Proj __proj = {}) const
      {
 return (*this)(ranges::begin(__r), ranges::end(__r),
         std::move(__pred), std::move(__proj));
      }
  };

  inline constexpr __find_if_not_fn find_if_not{};

  template<typename _Iter1, typename _Iter2>
    struct in_in_result
    {
      [[no_unique_address]] _Iter1 in1;
      [[no_unique_address]] _Iter2 in2;

      template<typename _IIter1, typename _IIter2>
 requires convertible_to<const _Iter1&, _IIter1>
   && convertible_to<const _Iter2&, _IIter2>
 constexpr
 operator in_in_result<_IIter1, _IIter2>() const &
 { return {in1, in2}; }

      template<typename _IIter1, typename _IIter2>
 requires convertible_to<_Iter1, _IIter1>
   && convertible_to<_Iter2, _IIter2>
 constexpr
 operator in_in_result<_IIter1, _IIter2>() &&
 { return {std::move(in1), std::move(in2)}; }
    };

  template<typename _Iter1, typename _Iter2>
    using mismatch_result = in_in_result<_Iter1, _Iter2>;

  struct __mismatch_fn
  {
    template<input_iterator _Iter1, sentinel_for<_Iter1> _Sent1,
      input_iterator _Iter2, sentinel_for<_Iter2> _Sent2,
      typename _Pred = ranges::equal_to,
      typename _Proj1 = identity, typename _Proj2 = identity>
      requires indirectly_comparable<_Iter1, _Iter2, _Pred, _Proj1, _Proj2>
      constexpr mismatch_result<_Iter1, _Iter2>
      operator()(_Iter1 __first1, _Sent1 __last1,
   _Iter2 __first2, _Sent2 __last2, _Pred __pred = {},
   _Proj1 __proj1 = {}, _Proj2 __proj2 = {}) const
      {
 while (__first1 != __last1 && __first2 != __last2
        && (bool)std::__invoke(__pred,
          std::__invoke(__proj1, *__first1),
          std::__invoke(__proj2, *__first2)))
 {
   ++__first1;
   ++__first2;
 }
 return { std::move(__first1), std::move(__first2) };
      }

    template<input_range _Range1, input_range _Range2,
      typename _Pred = ranges::equal_to,
      typename _Proj1 = identity, typename _Proj2 = identity>
      requires indirectly_comparable<iterator_t<_Range1>, iterator_t<_Range2>,
         _Pred, _Proj1, _Proj2>
      constexpr mismatch_result<iterator_t<_Range1>, iterator_t<_Range2>>
      operator()(_Range1&& __r1, _Range2&& __r2, _Pred __pred = {},
   _Proj1 __proj1 = {}, _Proj2 __proj2 = {}) const
      {
 return (*this)(ranges::begin(__r1), ranges::end(__r1),
         ranges::begin(__r2), ranges::end(__r2),
         std::move(__pred),
         std::move(__proj1), std::move(__proj2));
      }
  };

  inline constexpr __mismatch_fn mismatch{};

  struct __search_fn
  {
    template<forward_iterator _Iter1, sentinel_for<_Iter1> _Sent1,
      forward_iterator _Iter2, sentinel_for<_Iter2> _Sent2,
      typename _Pred = ranges::equal_to,
      typename _Proj1 = identity, typename _Proj2 = identity>
      requires indirectly_comparable<_Iter1, _Iter2, _Pred, _Proj1, _Proj2>
      constexpr subrange<_Iter1>
      operator()(_Iter1 __first1, _Sent1 __last1,
   _Iter2 __first2, _Sent2 __last2, _Pred __pred = {},
   _Proj1 __proj1 = {}, _Proj2 __proj2 = {}) const
      {
 if (__first1 == __last1 || __first2 == __last2)
   return {__first1, __first1};

 for (;;)
   {
     for (;;)
       {
  if (__first1 == __last1)
    return {__first1, __first1};
  if (std::__invoke(__pred,
      std::__invoke(__proj1, *__first1),
      std::__invoke(__proj2, *__first2)))
    break;
  ++__first1;
       }
     auto __cur1 = __first1;
     auto __cur2 = __first2;
     for (;;)
       {
  if (++__cur2 == __last2)
    return {__first1, ++__cur1};
  if (++__cur1 == __last1)
    return {__cur1, __cur1};
  if (!(bool)std::__invoke(__pred,
      std::__invoke(__proj1, *__cur1),
      std::__invoke(__proj2, *__cur2)))
    {
      ++__first1;
      break;
    }
       }
   }
      }

    template<forward_range _Range1, forward_range _Range2,
      typename _Pred = ranges::equal_to,
      typename _Proj1 = identity, typename _Proj2 = identity>
      requires indirectly_comparable<iterator_t<_Range1>, iterator_t<_Range2>,
         _Pred, _Proj1, _Proj2>
      constexpr borrowed_subrange_t<_Range1>
      operator()(_Range1&& __r1, _Range2&& __r2, _Pred __pred = {},
   _Proj1 __proj1 = {}, _Proj2 __proj2 = {}) const
      {
 return (*this)(ranges::begin(__r1), ranges::end(__r1),
         ranges::begin(__r2), ranges::end(__r2),
         std::move(__pred),
         std::move(__proj1), std::move(__proj2));
      }
  };

  inline constexpr __search_fn search{};

  struct __min_fn
  {
    template<typename _Tp, typename _Proj = identity,
      indirect_strict_weak_order<projected<const _Tp*, _Proj>>
        _Comp = ranges::less>
      constexpr const _Tp&
      operator()(const _Tp& __a, const _Tp& __b,
   _Comp __comp = {}, _Proj __proj = {}) const
      {
 if (std::__invoke(__comp,
     std::__invoke(__proj, __b),
     std::__invoke(__proj, __a)))
   return __b;
 else
   return __a;
      }

    template<input_range _Range, typename _Proj = identity,
      indirect_strict_weak_order<projected<iterator_t<_Range>, _Proj>>
        _Comp = ranges::less>
      requires indirectly_copyable_storable<iterator_t<_Range>,
         range_value_t<_Range>*>
      constexpr range_value_t<_Range>
      operator()(_Range&& __r, _Comp __comp = {}, _Proj __proj = {}) const
      {
 auto __first = ranges::begin(__r);
 auto __last = ranges::end(__r);
 do { if (__builtin_expect(!bool(__first != __last), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ranges_util.h", 756, __PRETTY_FUNCTION__, "__first != __last"); } while (false);
 auto __result = *__first;
 while (++__first != __last)
   {
     auto&& __tmp = *__first;
     if (std::__invoke(__comp,
         std::__invoke(__proj, __tmp),
         std::__invoke(__proj, __result)))
       __result = std::forward<decltype(__tmp)>(__tmp);
   }
 return __result;
      }

    template<copyable _Tp, typename _Proj = identity,
      indirect_strict_weak_order<projected<const _Tp*, _Proj>>
        _Comp = ranges::less>
      constexpr _Tp
      operator()(initializer_list<_Tp> __r,
   _Comp __comp = {}, _Proj __proj = {}) const
      {
 return (*this)(ranges::subrange(__r),
         std::move(__comp), std::move(__proj));
      }
  };

  inline constexpr __min_fn min{};

  struct __adjacent_find_fn
  {
    template<forward_iterator _Iter, sentinel_for<_Iter> _Sent,
      typename _Proj = identity,
      indirect_binary_predicate<projected<_Iter, _Proj>,
           projected<_Iter, _Proj>> _Pred
        = ranges::equal_to>
      constexpr _Iter
      operator()(_Iter __first, _Sent __last,
   _Pred __pred = {}, _Proj __proj = {}) const
      {
 if (__first == __last)
   return __first;
 auto __next = __first;
 for (; ++__next != __last; __first = __next)
   {
     if (std::__invoke(__pred,
         std::__invoke(__proj, *__first),
         std::__invoke(__proj, *__next)))
       return __first;
   }
 return __next;
      }

    template<forward_range _Range, typename _Proj = identity,
      indirect_binary_predicate<
        projected<iterator_t<_Range>, _Proj>,
        projected<iterator_t<_Range>, _Proj>> _Pred = ranges::equal_to>
      constexpr borrowed_iterator_t<_Range>
      operator()(_Range&& __r, _Pred __pred = {}, _Proj __proj = {}) const
      {
 return (*this)(ranges::begin(__r), ranges::end(__r),
         std::move(__pred), std::move(__proj));
      }
  };

  inline constexpr __adjacent_find_fn adjacent_find{};

}

  using ranges::get;

  template<typename _Iter, typename _Sent, ranges::subrange_kind _Kind>
    struct tuple_size<ranges::subrange<_Iter, _Sent, _Kind>>
    : integral_constant<size_t, 2>
    { };

  template<typename _Iter, typename _Sent, ranges::subrange_kind _Kind>
    struct tuple_element<0, ranges::subrange<_Iter, _Sent, _Kind>>
    { using type = _Iter; };

  template<typename _Iter, typename _Sent, ranges::subrange_kind _Kind>
    struct tuple_element<1, ranges::subrange<_Iter, _Sent, _Kind>>
    { using type = _Sent; };

  template<typename _Iter, typename _Sent, ranges::subrange_kind _Kind>
    struct tuple_element<0, const ranges::subrange<_Iter, _Sent, _Kind>>
    { using type = _Iter; };

  template<typename _Iter, typename _Sent, ranges::subrange_kind _Kind>
    struct tuple_element<1, const ranges::subrange<_Iter, _Sent, _Kind>>
    { using type = _Sent; };


}
# 47 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 2 3
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

      constexpr
      _Head_base(allocator_arg_t, __uses_alloc0)
      : _M_head_impl() { }

      template<typename _Alloc>
 constexpr
 _Head_base(allocator_arg_t, __uses_alloc1<_Alloc> __a)
 : _M_head_impl(allocator_arg, *__a._M_a) { }

      template<typename _Alloc>
 constexpr
 _Head_base(allocator_arg_t, __uses_alloc2<_Alloc> __a)
 : _M_head_impl(*__a._M_a) { }

      template<typename _UHead>
 constexpr
 _Head_base(__uses_alloc0, _UHead&& __uhead)
 : _M_head_impl(std::forward<_UHead>(__uhead)) { }

      template<typename _Alloc, typename _UHead>
 constexpr
 _Head_base(__uses_alloc1<_Alloc> __a, _UHead&& __uhead)
 : _M_head_impl(allocator_arg, *__a._M_a, std::forward<_UHead>(__uhead))
 { }

      template<typename _Alloc, typename _UHead>
 constexpr
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

      constexpr
      _Head_base(allocator_arg_t, __uses_alloc0)
      : _M_head_impl() { }

      template<typename _Alloc>
 constexpr
 _Head_base(allocator_arg_t, __uses_alloc1<_Alloc> __a)
 : _M_head_impl(allocator_arg, *__a._M_a) { }

      template<typename _Alloc>
 constexpr
 _Head_base(allocator_arg_t, __uses_alloc2<_Alloc> __a)
 : _M_head_impl(*__a._M_a) { }

      template<typename _UHead>
 constexpr
 _Head_base(__uses_alloc0, _UHead&& __uhead)
 : _M_head_impl(std::forward<_UHead>(__uhead)) { }

      template<typename _Alloc, typename _UHead>
 constexpr
 _Head_base(__uses_alloc1<_Alloc> __a, _UHead&& __uhead)
 : _M_head_impl(allocator_arg, *__a._M_a, std::forward<_UHead>(__uhead))
 { }

      template<typename _Alloc, typename _UHead>
 constexpr
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
 constexpr
 _Tuple_impl(allocator_arg_t __tag, const _Alloc& __a)
 : _Inherited(__tag, __a),
   _Base(__tag, __use_alloc<_Head>(__a))
 { }

      template<typename _Alloc>
 constexpr
 _Tuple_impl(allocator_arg_t __tag, const _Alloc& __a,
      const _Head& __head, const _Tail&... __tail)
 : _Inherited(__tag, __a, __tail...),
   _Base(__use_alloc<_Head, _Alloc, _Head>(__a), __head)
 { }

      template<typename _Alloc, typename _UHead, typename... _UTail,
        typename = __enable_if_t<sizeof...(_Tail) == sizeof...(_UTail)>>
 constexpr
 _Tuple_impl(allocator_arg_t __tag, const _Alloc& __a,
      _UHead&& __head, _UTail&&... __tail)
 : _Inherited(__tag, __a, std::forward<_UTail>(__tail)...),
   _Base(__use_alloc<_Head, _Alloc, _UHead>(__a),
  std::forward<_UHead>(__head))
 { }

      template<typename _Alloc>
 constexpr
 _Tuple_impl(allocator_arg_t __tag, const _Alloc& __a,
      const _Tuple_impl& __in)
 : _Inherited(__tag, __a, _M_tail(__in)),
   _Base(__use_alloc<_Head, _Alloc, _Head>(__a), _M_head(__in))
 { }

      template<typename _Alloc>
 constexpr
 _Tuple_impl(allocator_arg_t __tag, const _Alloc& __a,
      _Tuple_impl&& __in)
 : _Inherited(__tag, __a, std::move(_M_tail(__in))),
   _Base(__use_alloc<_Head, _Alloc, _Head>(__a),
  std::forward<_Head>(_M_head(__in)))
 { }

      template<typename _Alloc, typename _UHead, typename... _UTails>
 constexpr
 _Tuple_impl(allocator_arg_t __tag, const _Alloc& __a,
      const _Tuple_impl<_Idx, _UHead, _UTails...>& __in)
 : _Inherited(__tag, __a,
       _Tuple_impl<_Idx, _UHead, _UTails...>::_M_tail(__in)),
   _Base(__use_alloc<_Head, _Alloc, const _UHead&>(__a),
  _Tuple_impl<_Idx, _UHead, _UTails...>::_M_head(__in))
 { }

      template<typename _Alloc, typename _UHead, typename... _UTails>
 constexpr
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
 constexpr
 void
 _M_assign(const _Tuple_impl<_Idx, _UElements...>& __in)
 {
   _M_head(*this) = _Tuple_impl<_Idx, _UElements...>::_M_head(__in);
   _M_tail(*this)._M_assign(
       _Tuple_impl<_Idx, _UElements...>::_M_tail(__in));
 }

      template<typename _UHead, typename... _UTails>
 constexpr
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
      constexpr
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
 constexpr
 _Tuple_impl(allocator_arg_t __tag, const _Alloc& __a)
 : _Base(__tag, __use_alloc<_Head>(__a))
 { }

      template<typename _Alloc>
 constexpr
 _Tuple_impl(allocator_arg_t, const _Alloc& __a,
      const _Head& __head)
 : _Base(__use_alloc<_Head, _Alloc, const _Head&>(__a), __head)
 { }

      template<typename _Alloc, typename _UHead>
 constexpr
 _Tuple_impl(allocator_arg_t, const _Alloc& __a,
      _UHead&& __head)
 : _Base(__use_alloc<_Head, _Alloc, _UHead>(__a),
  std::forward<_UHead>(__head))
 { }

      template<typename _Alloc>
 constexpr
 _Tuple_impl(allocator_arg_t, const _Alloc& __a,
      const _Tuple_impl& __in)
 : _Base(__use_alloc<_Head, _Alloc, const _Head&>(__a), _M_head(__in))
 { }

      template<typename _Alloc>
 constexpr
 _Tuple_impl(allocator_arg_t, const _Alloc& __a,
      _Tuple_impl&& __in)
 : _Base(__use_alloc<_Head, _Alloc, _Head>(__a),
  std::forward<_Head>(_M_head(__in)))
 { }

      template<typename _Alloc, typename _UHead>
 constexpr
 _Tuple_impl(allocator_arg_t, const _Alloc& __a,
      const _Tuple_impl<_Idx, _UHead>& __in)
 : _Base(__use_alloc<_Head, _Alloc, const _UHead&>(__a),
  _Tuple_impl<_Idx, _UHead>::_M_head(__in))
 { }

      template<typename _Alloc, typename _UHead>
 constexpr
 _Tuple_impl(allocator_arg_t, const _Alloc& __a,
      _Tuple_impl<_Idx, _UHead>&& __in)
 : _Base(__use_alloc<_Head, _Alloc, _UHead>(__a),
  std::forward<_UHead>(_Tuple_impl<_Idx, _UHead>::_M_head(__in)))
 { }
# 703 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
      template<typename _UHead>
 constexpr
 void
 _M_assign(const _Tuple_impl<_Idx, _UHead>& __in)
 {
   _M_head(*this) = _Tuple_impl<_Idx, _UHead>::_M_head(__in);
 }

      template<typename _UHead>
 constexpr
 void
 _M_assign(_Tuple_impl<_Idx, _UHead>&& __in)
 {
   _M_head(*this)
     = std::forward<_UHead>(_Tuple_impl<_Idx, _UHead>::_M_head(__in));
 }
# 749 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
    protected:
      constexpr
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


      template<typename... _UTypes>
 static consteval bool
 __constructible()
 {
   if constexpr (sizeof...(_UTypes) == sizeof...(_Elements))
     return __and_v<is_constructible<_Elements, _UTypes>...>;
   else
     return false;
 }

      template<typename... _UTypes>
 static consteval bool
 __nothrow_constructible()
 {
   if constexpr (sizeof...(_UTypes) == sizeof...(_Elements))
     return __and_v<is_nothrow_constructible<_Elements, _UTypes>...>;
   else
     return false;
 }

      template<typename... _UTypes>
 static consteval bool
 __convertible()
 {
   if constexpr (sizeof...(_UTypes) == sizeof...(_Elements))
     return __and_v<is_convertible<_UTypes, _Elements>...>;
   else
     return false;
 }



      template<typename... _UTypes>
 static consteval bool
 __disambiguating_constraint()
 {
   if constexpr (sizeof...(_Elements) != sizeof...(_UTypes))
     return false;
   else if constexpr (sizeof...(_Elements) == 1)
     {
       using _U0 = typename _Nth_type<0, _UTypes...>::type;
       return !is_same_v<remove_cvref_t<_U0>, tuple>;
     }
   else if constexpr (sizeof...(_Elements) < 4)
     {
       using _U0 = typename _Nth_type<0, _UTypes...>::type;
       if constexpr (!is_same_v<remove_cvref_t<_U0>, allocator_arg_t>)
  return true;
       else
  {
    using _T0 = typename _Nth_type<0, _Elements...>::type;
    return is_same_v<remove_cvref_t<_T0>, allocator_arg_t>;
  }
     }
   return true;
 }




      template<typename _Tuple>
 static consteval bool
 __use_other_ctor()
 {
   if constexpr (sizeof...(_Elements) != 1)
     return false;
   else if constexpr (is_same_v<remove_cvref_t<_Tuple>, tuple>)
     return true;
   else
     {
       using _Tp = typename _Nth_type<0, _Elements...>::type;
       if constexpr (is_convertible_v<_Tuple, _Tp>)
  return true;
       else if constexpr (is_constructible_v<_Tp, _Tuple>)
  return true;
     }
   return false;
 }

      template<typename... _Up>
 static consteval bool
 __dangles()
 {

   return (__reference_constructs_from_temporary(_Elements, _Up&&)
      || ...);



 }
# 960 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
    public:
      constexpr
      explicit(!(__is_implicitly_default_constructible_v<_Elements> && ...))
      tuple()
      noexcept((is_nothrow_default_constructible_v<_Elements> && ...))
      requires (is_default_constructible_v<_Elements> && ...)
      : _Inherited()
      { }

      constexpr explicit(!__convertible<const _Elements&...>())
      tuple(const _Elements&... __elements)
      noexcept(__nothrow_constructible<const _Elements&...>())
      requires (__constructible<const _Elements&...>())
      : _Inherited(__elements...)
      { }

      template<typename... _UTypes>
 requires (__disambiguating_constraint<_UTypes...>())
   && (__constructible<_UTypes...>())
   && (!__dangles<_UTypes...>())
 constexpr explicit(!__convertible<_UTypes...>())
 tuple(_UTypes&&... __u)
 noexcept(__nothrow_constructible<_UTypes...>())
 : _Inherited(std::forward<_UTypes>(__u)...)
 { }

      template<typename... _UTypes>
 requires (__disambiguating_constraint<_UTypes...>())
   && (__constructible<_UTypes...>())
   && (__dangles<_UTypes...>())
 tuple(_UTypes&&...) = delete;

      constexpr tuple(const tuple&) = default;

      constexpr tuple(tuple&&) = default;

      template<typename... _UTypes>
 requires (__constructible<const _UTypes&...>())
   && (!__use_other_ctor<const tuple<_UTypes...>&>())
   && (!__dangles<const _UTypes&...>())
 constexpr explicit(!__convertible<const _UTypes&...>())
 tuple(const tuple<_UTypes...>& __u)
 noexcept(__nothrow_constructible<const _UTypes&...>())
 : _Inherited(static_cast<const _Tuple_impl<0, _UTypes...>&>(__u))
 { }

      template<typename... _UTypes>
 requires (__constructible<const _UTypes&...>())
   && (!__use_other_ctor<const tuple<_UTypes...>&>())
   && (__dangles<const _UTypes&...>())
 tuple(const tuple<_UTypes...>&) = delete;

      template<typename... _UTypes>
 requires (__constructible<_UTypes...>())
   && (!__use_other_ctor<tuple<_UTypes...>>())
   && (!__dangles<_UTypes...>())
 constexpr explicit(!__convertible<_UTypes...>())
 tuple(tuple<_UTypes...>&& __u)
 noexcept(__nothrow_constructible<_UTypes...>())
 : _Inherited(static_cast<_Tuple_impl<0, _UTypes...>&&>(__u))
 { }

      template<typename... _UTypes>
 requires (__constructible<_UTypes...>())
   && (!__use_other_ctor<tuple<_UTypes...>>())
   && (__dangles<_UTypes...>())
 tuple(tuple<_UTypes...>&&) = delete;
# 1062 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
      template<typename _U1, typename _U2>
 requires (sizeof...(_Elements) == 2)
   && (__constructible<const _U1&, const _U2&>())
   && (!__dangles<const _U1&, const _U2&>())
 constexpr explicit(!__convertible<const _U1&, const _U2&>())
 tuple(const pair<_U1, _U2>& __u)
 noexcept(__nothrow_constructible<const _U1&, const _U2&>())
 : _Inherited(__u.first, __u.second)
 { }

      template<typename _U1, typename _U2>
 requires (sizeof...(_Elements) == 2)
   && (__constructible<const _U1&, const _U2&>())
   && (__dangles<const _U1&, const _U2&>())
 tuple(const pair<_U1, _U2>&) = delete;

      template<typename _U1, typename _U2>
 requires (sizeof...(_Elements) == 2)
   && (__constructible<_U1, _U2>())
   && (!__dangles<_U1, _U2>())
 constexpr explicit(!__convertible<_U1, _U2>())
 tuple(pair<_U1, _U2>&& __u)
 noexcept(__nothrow_constructible<_U1, _U2>())
 : _Inherited(std::forward<_U1>(__u.first),
       std::forward<_U2>(__u.second))
 { }

      template<typename _U1, typename _U2>
 requires (sizeof...(_Elements) == 2)
   && (__constructible<_U1, _U2>())
   && (__dangles<_U1, _U2>())
 tuple(pair<_U1, _U2>&&) = delete;
# 1151 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
      template<typename _Alloc>
 constexpr
 explicit(!(__is_implicitly_default_constructible_v<_Elements> && ...))
 tuple(allocator_arg_t __tag, const _Alloc& __a)
 requires (is_default_constructible_v<_Elements> && ...)
 : _Inherited(__tag, __a)
 { }

      template<typename _Alloc>
 constexpr explicit(!__convertible<const _Elements&...>())
 tuple(allocator_arg_t __tag, const _Alloc& __a,
       const _Elements&... __elements)
 requires (__constructible<const _Elements&...>())
 : _Inherited(__tag, __a, __elements...)
 { }

      template<typename _Alloc, typename... _UTypes>
 requires (__disambiguating_constraint<_UTypes...>())
   && (__constructible<_UTypes...>())
   && (!__dangles<_UTypes...>())
 constexpr explicit(!__convertible<_UTypes...>())
 tuple(allocator_arg_t __tag, const _Alloc& __a, _UTypes&&... __u)
 : _Inherited(__tag, __a, std::forward<_UTypes>(__u)...)
 { }

      template<typename _Alloc, typename... _UTypes>
 requires (__disambiguating_constraint<_UTypes...>())
   && (__constructible<_UTypes...>())
   && (__dangles<_UTypes...>())
 tuple(allocator_arg_t, const _Alloc&, _UTypes&&...) = delete;

      template<typename _Alloc>
 constexpr
 tuple(allocator_arg_t __tag, const _Alloc& __a, const tuple& __u)
 : _Inherited(__tag, __a, static_cast<const _Inherited&>(__u))
 { }

      template<typename _Alloc>
 requires (__constructible<_Elements...>())
 constexpr
 tuple(allocator_arg_t __tag, const _Alloc& __a, tuple&& __u)
 : _Inherited(__tag, __a, static_cast<_Inherited&&>(__u))
 { }

      template<typename _Alloc, typename... _UTypes>
 requires (__constructible<const _UTypes&...>())
   && (!__use_other_ctor<const tuple<_UTypes...>&>())
   && (!__dangles<const _UTypes&...>())
 constexpr explicit(!__convertible<const _UTypes&...>())
 tuple(allocator_arg_t __tag, const _Alloc& __a,
       const tuple<_UTypes...>& __u)
 : _Inherited(__tag, __a,
       static_cast<const _Tuple_impl<0, _UTypes...>&>(__u))
 { }

      template<typename _Alloc, typename... _UTypes>
 requires (__constructible<const _UTypes&...>())
   && (!__use_other_ctor<const tuple<_UTypes...>&>())
   && (__dangles<const _UTypes&...>())
 tuple(allocator_arg_t, const _Alloc&, const tuple<_UTypes...>&) = delete;

      template<typename _Alloc, typename... _UTypes>
 requires (__constructible<_UTypes...>())
   && (!__use_other_ctor<tuple<_UTypes...>>())
   && (!__dangles<_UTypes...>())
 constexpr explicit(!__use_other_ctor<tuple<_UTypes...>>())
 tuple(allocator_arg_t __tag, const _Alloc& __a, tuple<_UTypes...>&& __u)
 : _Inherited(__tag, __a, static_cast<_Tuple_impl<0, _UTypes...>&&>(__u))
 { }

      template<typename _Alloc, typename... _UTypes>
 requires (__constructible<_UTypes...>())
   && (!__use_other_ctor<tuple<_UTypes...>>())
   && (__dangles<_UTypes...>())
 tuple(allocator_arg_t, const _Alloc&, tuple<_UTypes...>&&) = delete;
# 1261 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
      template<typename _Alloc, typename _U1, typename _U2>
 requires (sizeof...(_Elements) == 2)
   && (__constructible<const _U1&, const _U2&>())
   && (!__dangles<const _U1&, const _U2&>())
 constexpr explicit(!__convertible<const _U1&, const _U2&>())
 tuple(allocator_arg_t __tag, const _Alloc& __a,
       const pair<_U1, _U2>& __u)
 noexcept(__nothrow_constructible<const _U1&, const _U2&>())
 : _Inherited(__tag, __a, __u.first, __u.second)
 { }

      template<typename _Alloc, typename _U1, typename _U2>
 requires (sizeof...(_Elements) == 2)
   && (__constructible<const _U1&, const _U2&>())
   && (__dangles<const _U1&, const _U2&>())
 tuple(allocator_arg_t, const _Alloc&, const pair<_U1, _U2>&) = delete;

      template<typename _Alloc, typename _U1, typename _U2>
 requires (sizeof...(_Elements) == 2)
   && (__constructible<_U1, _U2>())
   && (!__dangles<_U1, _U2>())
 constexpr explicit(!__convertible<_U1, _U2>())
 tuple(allocator_arg_t __tag, const _Alloc& __a, pair<_U1, _U2>&& __u)
 noexcept(__nothrow_constructible<_U1, _U2>())
 : _Inherited(__tag, __a, std::move(__u.first), std::move(__u.second))
 { }

      template<typename _Alloc, typename _U1, typename _U2>
 requires (sizeof...(_Elements) == 2)
   && (__constructible<_U1, _U2>())
   && (__dangles<_U1, _U2>())
 tuple(allocator_arg_t, const _Alloc&, pair<_U1, _U2>&&) = delete;
# 1653 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
    private:
      template<typename... _UTypes>
 static consteval bool
 __assignable()
 {
   if constexpr (sizeof...(_UTypes) == sizeof...(_Elements))
     return __and_v<is_assignable<_Elements&, _UTypes>...>;
   else
     return false;
 }

      template<typename... _UTypes>
 static consteval bool
 __nothrow_assignable()
 {
   if constexpr (sizeof...(_UTypes) == sizeof...(_Elements))
     return __and_v<is_nothrow_assignable<_Elements&, _UTypes>...>;
   else
     return false;
 }
# 1706 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
    public:

      tuple& operator=(const tuple& __u) = delete;

      constexpr tuple&
      operator=(const tuple& __u)
      noexcept(__nothrow_assignable<const _Elements&...>())
      requires (__assignable<const _Elements&...>())
      {
 this->_M_assign(__u);
 return *this;
      }

      constexpr tuple&
      operator=(tuple&& __u)
      noexcept(__nothrow_assignable<_Elements...>())
      requires (__assignable<_Elements...>())
      {
 this->_M_assign(std::move(__u));
 return *this;
      }

      template<typename... _UTypes>
 requires (__assignable<const _UTypes&...>())
 constexpr tuple&
 operator=(const tuple<_UTypes...>& __u)
 noexcept(__nothrow_assignable<const _UTypes&...>())
 {
   this->_M_assign(__u);
   return *this;
 }

      template<typename... _UTypes>
 requires (__assignable<_UTypes...>())
 constexpr tuple&
 operator=(tuple<_UTypes...>&& __u)
 noexcept(__nothrow_assignable<_UTypes...>())
 {
   this->_M_assign(std::move(__u));
   return *this;
 }
# 1784 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
      template<typename _U1, typename _U2>
 requires (__assignable<const _U1&, const _U2&>())
 constexpr tuple&
 operator=(const pair<_U1, _U2>& __u)
 noexcept(__nothrow_assignable<const _U1&, const _U2&>())
 {
   this->_M_head(*this) = __u.first;
   this->_M_tail(*this)._M_head(*this) = __u.second;
   return *this;
 }

      template<typename _U1, typename _U2>
 requires (__assignable<_U1, _U2>())
 constexpr tuple&
 operator=(pair<_U1, _U2>&& __u)
 noexcept(__nothrow_assignable<_U1, _U2>())
 {
   this->_M_head(*this) = std::forward<_U1>(__u.first);
   this->_M_tail(*this)._M_head(*this) = std::forward<_U2>(__u.second);
   return *this;
 }
# 1946 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
      constexpr
      void
      swap(tuple& __in)
      noexcept(__and_<__is_nothrow_swappable<_Elements>...>::value)
      { _Inherited::_M_swap(__in); }
# 1965 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
    };


  template<typename... _UTypes>
    tuple(_UTypes...) -> tuple<_UTypes...>;
  template<typename _T1, typename _T2>
    tuple(pair<_T1, _T2>) -> tuple<_T1, _T2>;
  template<typename _Alloc, typename... _UTypes>
    tuple(allocator_arg_t, _Alloc, _UTypes...) -> tuple<_UTypes...>;
  template<typename _Alloc, typename _T1, typename _T2>
    tuple(allocator_arg_t, _Alloc, pair<_T1, _T2>) -> tuple<_T1, _T2>;
  template<typename _Alloc, typename... _UTypes>
    tuple(allocator_arg_t, _Alloc, tuple<_UTypes...>) -> tuple<_UTypes...>;



  template<>
    class tuple<>
    {
    public:
      constexpr
      void swap(tuple&) noexcept { }





      tuple() = default;

      template<typename _Alloc>
 constexpr
 tuple(allocator_arg_t, const _Alloc&) noexcept { }
      template<typename _Alloc>
 constexpr
 tuple(allocator_arg_t, const _Alloc&, const tuple&) noexcept { }
    };
# 2401 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
  template<typename... _Elements>
    struct tuple_size<tuple<_Elements...>>
    : public integral_constant<size_t, sizeof...(_Elements)> { };


  template<typename... _Types>
    inline constexpr size_t tuple_size_v<tuple<_Types...>>
      = sizeof...(_Types);

  template<typename... _Types>
    inline constexpr size_t tuple_size_v<const tuple<_Types...>>
      = sizeof...(_Types);



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




  template <typename _Tp, typename... _Types>
    constexpr _Tp&
    get(tuple<_Types...>& __t) noexcept
    {
      constexpr size_t __idx = __find_uniq_type_in_pack<_Tp, _Types...>();
      static_assert(__idx < sizeof...(_Types),
   "the type T in std::get<T> must occur exactly once in the tuple");
      return std::__get_helper<__idx>(__t);
    }


  template <typename _Tp, typename... _Types>
    constexpr _Tp&&
    get(tuple<_Types...>&& __t) noexcept
    {
      constexpr size_t __idx = __find_uniq_type_in_pack<_Tp, _Types...>();
      static_assert(__idx < sizeof...(_Types),
   "the type T in std::get<T> must occur exactly once in the tuple");
      return std::forward<_Tp>(std::__get_helper<__idx>(__t));
    }


  template <typename _Tp, typename... _Types>
    constexpr const _Tp&
    get(const tuple<_Types...>& __t) noexcept
    {
      constexpr size_t __idx = __find_uniq_type_in_pack<_Tp, _Types...>();
      static_assert(__idx < sizeof...(_Types),
   "the type T in std::get<T> must occur exactly once in the tuple");
      return std::__get_helper<__idx>(__t);
    }



  template <typename _Tp, typename... _Types>
    constexpr const _Tp&&
    get(const tuple<_Types...>&& __t) noexcept
    {
      constexpr size_t __idx = __find_uniq_type_in_pack<_Tp, _Types...>();
      static_assert(__idx < sizeof...(_Types),
   "the type T in std::get<T> must occur exactly once in the tuple");
      return std::forward<const _Tp>(std::__get_helper<__idx>(__t));
    }



  template<typename... _Tps, typename... _Ups>
    requires (sizeof...(_Tps) == sizeof...(_Ups))
      && (requires (const _Tps& __t, const _Ups& __u) {
 { __t == __u } -> __detail::__boolean_testable;
      } && ...)
    constexpr bool
    operator== [[nodiscard]] (const tuple<_Tps...>& __t,
         const tuple<_Ups...>& __u)
    {
      return [&]<size_t... _Inds>(index_sequence<_Inds...>) {

 return ((std::get<_Inds>(__t) == std::get<_Inds>(__u)) && ...);
      }(index_sequence_for<_Tps...>{});
    }

  template<typename _Cat, typename _Tp, typename _Up, typename _IndexSeq>
    [[nodiscard]]
    constexpr _Cat
    __tuple_cmp(const _Tp& __t, const _Up& __u, _IndexSeq __indices)
    {
      _Cat __c = _Cat::equivalent;



      auto __cmp = [&]<size_t _Ind>(integral_constant<size_t, _Ind>) {
 __c = __detail::__synth3way(std::get<_Ind>(__t), std::get<_Ind>(__u));
 return __c == 0;
      };

      [&]<size_t... _Inds>(index_sequence<_Inds...>) {

 (void)(__cmp(integral_constant<size_t, _Inds>{}) && ...);
      }(__indices);

      return __c;
    }

  template<typename... _Tps, typename... _Ups>
    requires (sizeof...(_Tps) == sizeof...(_Ups))
      && (requires { typename __detail::__synth3way_t<_Tps, _Ups>; } && ...)
    constexpr
    common_comparison_category_t<__detail::__synth3way_t<_Tps, _Ups>...>
    operator<=> [[nodiscard]] (const tuple<_Tps...>& __t,
          const tuple<_Ups...>& __u)
    {
      using _Cat
 = common_comparison_category_t<__detail::__synth3way_t<_Tps, _Ups>...>;
      return std::__tuple_cmp<_Cat>(__t, __u, index_sequence_for<_Tps...>());
    }
# 2664 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
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
    constexpr
    inline


    typename enable_if<__and_<__is_swappable<_Elements>...>::value
      >::type



    swap(tuple<_Elements...>& __x, tuple<_Elements...>& __y)
    noexcept(noexcept(__x.swap(__y)))
    { __x.swap(__y); }
# 2846 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
  template<typename... _Elements>
    constexpr
    typename enable_if<!__and_<__is_swappable<_Elements>...>::value>::type
    swap(tuple<_Elements...>&, tuple<_Elements...>&) = delete;



  template<typename... _Types, typename _Alloc>
    struct uses_allocator<tuple<_Types...>, _Alloc> : true_type { };
# 2865 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
  template<class _T1, class _T2>
    template<typename... _Args1, typename... _Args2>
      constexpr
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
      constexpr inline
      pair<_T1, _T2>::
      pair(tuple<_Args1...>& __tuple1, tuple<_Args2...>& __tuple2,
    _Index_tuple<_Indexes1...>, _Index_tuple<_Indexes2...>)
      : first(std::forward<_Args1>(std::get<_Indexes1>(__tuple1))...),
 second(std::forward<_Args2>(std::get<_Indexes2>(__tuple2))...)
      { }






  template<template<typename...> class _Trait, typename _Tp, typename _Tuple>
    inline constexpr bool __unpack_std_tuple = false;

  template<template<typename...> class _Trait, typename _Tp, typename... _Up>
    inline constexpr bool __unpack_std_tuple<_Trait, _Tp, tuple<_Up...>>
      = _Trait<_Tp, _Up...>::value;

  template<template<typename...> class _Trait, typename _Tp, typename... _Up>
    inline constexpr bool __unpack_std_tuple<_Trait, _Tp, tuple<_Up...>&>
      = _Trait<_Tp, _Up&...>::value;

  template<template<typename...> class _Trait, typename _Tp, typename... _Up>
    inline constexpr bool __unpack_std_tuple<_Trait, _Tp, const tuple<_Up...>>
      = _Trait<_Tp, const _Up...>::value;

  template<template<typename...> class _Trait, typename _Tp, typename... _Up>
    inline constexpr bool __unpack_std_tuple<_Trait, _Tp, const tuple<_Up...>&>
      = _Trait<_Tp, const _Up&...>::value;



  template <typename _Fn, typename _Tuple, size_t... _Idx>
    constexpr decltype(auto)
    __apply_impl(_Fn&& __f, _Tuple&& __t, index_sequence<_Idx...>)
    {
      return std::__invoke(std::forward<_Fn>(__f),
      std::get<_Idx>(std::forward<_Tuple>(__t))...);
    }




  template <typename _Fn, typename _Tuple>

    constexpr decltype(auto)
    apply(_Fn&& __f, _Tuple&& __t)
    noexcept(__unpack_std_tuple<is_nothrow_invocable, _Fn, _Tuple>)
    {
      using _Indices
 = make_index_sequence<tuple_size_v<remove_reference_t<_Tuple>>>;
      return std::__apply_impl(std::forward<_Fn>(__f),
          std::forward<_Tuple>(__t),
          _Indices{});
    }



  template <typename _Tp, typename _Tuple, size_t... _Idx>
    constexpr _Tp
    __make_from_tuple_impl(_Tuple&& __t, index_sequence<_Idx...>)
    { return _Tp(std::get<_Idx>(std::forward<_Tuple>(__t))...); }




  template <typename _Tp, typename _Tuple>

    constexpr _Tp
    make_from_tuple(_Tuple&& __t)
    noexcept(__unpack_std_tuple<is_nothrow_constructible, _Tp, _Tuple>)
    {
      constexpr size_t __n = tuple_size_v<remove_reference_t<_Tuple>>;

      if constexpr (__n == 1)
 {
   using _Elt = decltype(std::get<0>(std::declval<_Tuple>()));
   static_assert(!__reference_constructs_from_temporary(_Tp, _Elt));
 }

      return __make_from_tuple_impl<_Tp>(std::forward<_Tuple>(__t),
      make_index_sequence<__n>{});
    }
# 3027 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3

}
# 5 "./cpp2a/pr99833.C" 2


# 6 "./cpp2a/pr99833.C"
auto f(auto&& x)
{
  [&](auto...) {
    auto y = std::tuple{ "what's happening here?", x };
    if constexpr (auto [_, z] = y; requires { z; })
      return;
  }();
}

int main()
{
  f(42);
}
