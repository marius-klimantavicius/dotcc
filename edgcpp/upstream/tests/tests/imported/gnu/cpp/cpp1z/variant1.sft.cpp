//type: fp
//options: --c++17
# 0 "./cpp1z/variant1.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./cpp1z/variant1.C"




# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/variant" 1 3
# 39 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/variant" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/version.h" 1 3
# 51 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/version.h" 3
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
# 52 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/version.h" 2 3
# 40 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/variant" 2 3


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
# 43 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/variant" 2 3
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
# 3619 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
template <typename _Tp>
  inline constexpr bool is_trivially_destructible_v =
    is_trivially_destructible<_Tp>::value;


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
# 4189 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3

}
}
# 44 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/variant" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/enable_special_members.h" 1 3
# 39 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/enable_special_members.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{



  struct _Enable_default_constructor_tag
  {
    explicit constexpr _Enable_default_constructor_tag() = default;
  };






template<bool _Switch, typename _Tag = void>
  struct _Enable_default_constructor
  {
    constexpr _Enable_default_constructor() noexcept = default;
    constexpr _Enable_default_constructor(_Enable_default_constructor const&)
      noexcept = default;
    constexpr _Enable_default_constructor(_Enable_default_constructor&&)
      noexcept = default;
    _Enable_default_constructor&
    operator=(_Enable_default_constructor const&) noexcept = default;
    _Enable_default_constructor&
    operator=(_Enable_default_constructor&&) noexcept = default;


    constexpr explicit
    _Enable_default_constructor(_Enable_default_constructor_tag) { }
  };







template<bool _Switch, typename _Tag = void>
  struct _Enable_destructor { };






template<bool _Copy, bool _CopyAssignment,
         bool _Move, bool _MoveAssignment,
         typename _Tag = void>
  struct _Enable_copy_move { };
# 98 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/enable_special_members.h" 3
template<bool _Default, bool _Destructor,
         bool _Copy, bool _CopyAssignment,
         bool _Move, bool _MoveAssignment,
         typename _Tag = void>
  struct _Enable_special_members
  : private _Enable_default_constructor<_Default, _Tag>,
    private _Enable_destructor<_Destructor, _Tag>,
    private _Enable_copy_move<_Copy, _CopyAssignment,
                              _Move, _MoveAssignment,
                              _Tag>
  { };



template<typename _Tag>
  struct _Enable_default_constructor<false, _Tag>
  {
    constexpr _Enable_default_constructor() noexcept = delete;
    constexpr _Enable_default_constructor(_Enable_default_constructor const&)
      noexcept = default;
    constexpr _Enable_default_constructor(_Enable_default_constructor&&)
      noexcept = default;
    _Enable_default_constructor&
    operator=(_Enable_default_constructor const&) noexcept = default;
    _Enable_default_constructor&
    operator=(_Enable_default_constructor&&) noexcept = default;


    constexpr explicit
    _Enable_default_constructor(_Enable_default_constructor_tag) { }
  };

template<typename _Tag>
  struct _Enable_destructor<false, _Tag>
  { ~_Enable_destructor() noexcept = delete; };

template<typename _Tag>
  struct _Enable_copy_move<false, true, true, true, _Tag>
  {
    constexpr _Enable_copy_move() noexcept = default;
    constexpr _Enable_copy_move(_Enable_copy_move const&) noexcept = delete;
    constexpr _Enable_copy_move(_Enable_copy_move&&) noexcept = default;
    _Enable_copy_move&
    operator=(_Enable_copy_move const&) noexcept = default;
    _Enable_copy_move&
    operator=(_Enable_copy_move&&) noexcept = default;
  };

template<typename _Tag>
  struct _Enable_copy_move<true, false, true, true, _Tag>
  {
    constexpr _Enable_copy_move() noexcept = default;
    constexpr _Enable_copy_move(_Enable_copy_move const&) noexcept = default;
    constexpr _Enable_copy_move(_Enable_copy_move&&) noexcept = default;
    _Enable_copy_move&
    operator=(_Enable_copy_move const&) noexcept = delete;
    _Enable_copy_move&
    operator=(_Enable_copy_move&&) noexcept = default;
  };

template<typename _Tag>
  struct _Enable_copy_move<false, false, true, true, _Tag>
  {
    constexpr _Enable_copy_move() noexcept = default;
    constexpr _Enable_copy_move(_Enable_copy_move const&) noexcept = delete;
    constexpr _Enable_copy_move(_Enable_copy_move&&) noexcept = default;
    _Enable_copy_move&
    operator=(_Enable_copy_move const&) noexcept = delete;
    _Enable_copy_move&
    operator=(_Enable_copy_move&&) noexcept = default;
  };

template<typename _Tag>
  struct _Enable_copy_move<true, true, false, true, _Tag>
  {
    constexpr _Enable_copy_move() noexcept = default;
    constexpr _Enable_copy_move(_Enable_copy_move const&) noexcept = default;
    constexpr _Enable_copy_move(_Enable_copy_move&&) noexcept = delete;
    _Enable_copy_move&
    operator=(_Enable_copy_move const&) noexcept = default;
    _Enable_copy_move&
    operator=(_Enable_copy_move&&) noexcept = default;
  };

template<typename _Tag>
  struct _Enable_copy_move<false, true, false, true, _Tag>
  {
    constexpr _Enable_copy_move() noexcept = default;
    constexpr _Enable_copy_move(_Enable_copy_move const&) noexcept = delete;
    constexpr _Enable_copy_move(_Enable_copy_move&&) noexcept = delete;
    _Enable_copy_move&
    operator=(_Enable_copy_move const&) noexcept = default;
    _Enable_copy_move&
    operator=(_Enable_copy_move&&) noexcept = default;
  };

template<typename _Tag>
  struct _Enable_copy_move<true, false, false, true, _Tag>
  {
    constexpr _Enable_copy_move() noexcept = default;
    constexpr _Enable_copy_move(_Enable_copy_move const&) noexcept = default;
    constexpr _Enable_copy_move(_Enable_copy_move&&) noexcept = delete;
    _Enable_copy_move&
    operator=(_Enable_copy_move const&) noexcept = delete;
    _Enable_copy_move&
    operator=(_Enable_copy_move&&) noexcept = default;
  };

template<typename _Tag>
  struct _Enable_copy_move<false, false, false, true, _Tag>
  {
    constexpr _Enable_copy_move() noexcept = default;
    constexpr _Enable_copy_move(_Enable_copy_move const&) noexcept = delete;
    constexpr _Enable_copy_move(_Enable_copy_move&&) noexcept = delete;
    _Enable_copy_move&
    operator=(_Enable_copy_move const&) noexcept = delete;
    _Enable_copy_move&
    operator=(_Enable_copy_move&&) noexcept = default;
  };

template<typename _Tag>
  struct _Enable_copy_move<true, true, true, false, _Tag>
  {
    constexpr _Enable_copy_move() noexcept = default;
    constexpr _Enable_copy_move(_Enable_copy_move const&) noexcept = default;
    constexpr _Enable_copy_move(_Enable_copy_move&&) noexcept = default;
    _Enable_copy_move&
    operator=(_Enable_copy_move const&) noexcept = default;
    _Enable_copy_move&
    operator=(_Enable_copy_move&&) noexcept = delete;
  };

template<typename _Tag>
  struct _Enable_copy_move<false, true, true, false, _Tag>
  {
    constexpr _Enable_copy_move() noexcept = default;
    constexpr _Enable_copy_move(_Enable_copy_move const&) noexcept = delete;
    constexpr _Enable_copy_move(_Enable_copy_move&&) noexcept = default;
    _Enable_copy_move&
    operator=(_Enable_copy_move const&) noexcept = default;
    _Enable_copy_move&
    operator=(_Enable_copy_move&&) noexcept = delete;
  };

template<typename _Tag>
  struct _Enable_copy_move<true, false, true, false, _Tag>
  {
    constexpr _Enable_copy_move() noexcept = default;
    constexpr _Enable_copy_move(_Enable_copy_move const&) noexcept = default;
    constexpr _Enable_copy_move(_Enable_copy_move&&) noexcept = default;
    _Enable_copy_move&
    operator=(_Enable_copy_move const&) noexcept = delete;
    _Enable_copy_move&
    operator=(_Enable_copy_move&&) noexcept = delete;
  };

template<typename _Tag>
  struct _Enable_copy_move<false, false, true, false, _Tag>
  {
    constexpr _Enable_copy_move() noexcept = default;
    constexpr _Enable_copy_move(_Enable_copy_move const&) noexcept = delete;
    constexpr _Enable_copy_move(_Enable_copy_move&&) noexcept = default;
    _Enable_copy_move&
    operator=(_Enable_copy_move const&) noexcept = delete;
    _Enable_copy_move&
    operator=(_Enable_copy_move&&) noexcept = delete;
  };

template<typename _Tag>
  struct _Enable_copy_move<true, true, false, false, _Tag>
  {
    constexpr _Enable_copy_move() noexcept = default;
    constexpr _Enable_copy_move(_Enable_copy_move const&) noexcept = default;
    constexpr _Enable_copy_move(_Enable_copy_move&&) noexcept = delete;
    _Enable_copy_move&
    operator=(_Enable_copy_move const&) noexcept = default;
    _Enable_copy_move&
    operator=(_Enable_copy_move&&) noexcept = delete;
  };

template<typename _Tag>
  struct _Enable_copy_move<false, true, false, false, _Tag>
  {
    constexpr _Enable_copy_move() noexcept = default;
    constexpr _Enable_copy_move(_Enable_copy_move const&) noexcept = delete;
    constexpr _Enable_copy_move(_Enable_copy_move&&) noexcept = delete;
    _Enable_copy_move&
    operator=(_Enable_copy_move const&) noexcept = default;
    _Enable_copy_move&
    operator=(_Enable_copy_move&&) noexcept = delete;
  };

template<typename _Tag>
  struct _Enable_copy_move<true, false, false, false, _Tag>
  {
    constexpr _Enable_copy_move() noexcept = default;
    constexpr _Enable_copy_move(_Enable_copy_move const&) noexcept = default;
    constexpr _Enable_copy_move(_Enable_copy_move&&) noexcept = delete;
    _Enable_copy_move&
    operator=(_Enable_copy_move const&) noexcept = delete;
    _Enable_copy_move&
    operator=(_Enable_copy_move&&) noexcept = delete;
  };

template<typename _Tag>
  struct _Enable_copy_move<false, false, false, false, _Tag>
  {
    constexpr _Enable_copy_move() noexcept = default;
    constexpr _Enable_copy_move(_Enable_copy_move const&) noexcept = delete;
    constexpr _Enable_copy_move(_Enable_copy_move&&) noexcept = delete;
    _Enable_copy_move&
    operator=(_Enable_copy_move const&) noexcept = delete;
    _Enable_copy_move&
    operator=(_Enable_copy_move&&) noexcept = delete;
  };



}
# 45 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/variant" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/exception_defines.h" 1 3
# 46 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/variant" 2 3
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

      typedef _Result result_type [[__deprecated__]];
      typedef _Arg argument_type [[__deprecated__]];

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


  template<>
    struct hash<nullptr_t> : public __hash_base<size_t, nullptr_t>
    {
      size_t
      operator()(nullptr_t) const noexcept
      { return 0; }
    };
# 297 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/functional_hash.h" 3
  template<typename _Hash>
    struct __is_fast_hash : public std::true_type
    { };

  template<>
    struct __is_fast_hash<hash<long double>> : public std::false_type
    { };


}
# 47 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/variant" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/invoke.h" 1 3
# 42 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/invoke.h" 3
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
# 43 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/invoke.h" 2 3

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
# 48 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/variant" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/monostate.h" 1 3
# 33 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/monostate.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/version.h" 1 3
# 34 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/monostate.h" 2 3
# 42 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/monostate.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{

  struct monostate { };

  constexpr bool operator==(monostate, monostate) noexcept { return true; }




  constexpr bool operator!=(monostate, monostate) noexcept { return false; }
  constexpr bool operator<(monostate, monostate) noexcept { return false; }
  constexpr bool operator>(monostate, monostate) noexcept { return false; }
  constexpr bool operator<=(monostate, monostate) noexcept { return true; }
  constexpr bool operator>=(monostate, monostate) noexcept { return true; }


  template<>
    struct hash<monostate>
    {

      using result_type [[__deprecated__]] = size_t;
      using argument_type [[__deprecated__]] = monostate;


      size_t
      operator()(const monostate&) const noexcept
      {
 constexpr size_t __magic_monostate_hash = -7777;
 return __magic_monostate_hash;
      }
    };


}
# 49 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/variant" 2 3
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


  enum class byte : unsigned char;

  template<>
    struct __is_byte<byte>
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
# 50 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/variant" 2 3
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
# 69 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_funcs.h" 2 3

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
# 51 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/variant" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_construct.h" 1 3
# 59 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_construct.h" 3
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
# 264 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/new" 3
}

#pragma GCC visibility pop
#pragma GCC diagnostic pop
# 60 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_construct.h" 2 3
# 73 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_construct.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{



  template <typename _Tp>
    inline void
    destroy_at(_Tp* __location)
    {
      if constexpr (201703L > 201703L && is_array_v<_Tp>)
 {
   for (auto& __x : *__location)
     std::destroy_at(std::__addressof(__x));
 }
      else
 __location->~_Tp();
    }
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


  template <typename _ForwardIterator>
    inline void
    destroy(_ForwardIterator __first, _ForwardIterator __last)
    {
      std::_Destroy(__first, __last);
    }

  template <typename _ForwardIterator, typename _Size>
    inline _ForwardIterator
    destroy_n(_ForwardIterator __first, _Size __count)
    {
      return std::_Destroy_n(__first, __count);
    }



}
# 52 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/variant" 2 3
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
# 284 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/utility.h" 3
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
# 53 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/variant" 2 3







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
# 61 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/variant" 2 3


namespace std __attribute__ ((__visibility__ ("default")))
{


  template<typename... _Types> class tuple;
  template<typename... _Types> class variant;

  template<typename _Variant>
    struct variant_size;

  template<typename _Variant>
    struct variant_size<const _Variant> : variant_size<_Variant> {};

  template<typename _Variant>
    struct variant_size<volatile _Variant> : variant_size<_Variant> {};

  template<typename _Variant>
    struct variant_size<const volatile _Variant> : variant_size<_Variant> {};

  template<typename... _Types>
    struct variant_size<variant<_Types...>>
    : std::integral_constant<size_t, sizeof...(_Types)> {};

  template<typename _Variant>
    inline constexpr size_t variant_size_v = variant_size<_Variant>::value;

  template<typename... _Types>
    inline constexpr size_t
    variant_size_v<variant<_Types...>> = sizeof...(_Types);

  template<typename... _Types>
    inline constexpr size_t
    variant_size_v<const variant<_Types...>> = sizeof...(_Types);

  template<size_t _Np, typename _Variant>
    struct variant_alternative;

  template<size_t _Np, typename... _Types>
    struct variant_alternative<_Np, variant<_Types...>>
    {
      static_assert(_Np < sizeof...(_Types));

      using type = typename _Nth_type<_Np, _Types...>::type;
    };

  template<size_t _Np, typename _Variant>
    using variant_alternative_t =
      typename variant_alternative<_Np, _Variant>::type;

  template<size_t _Np, typename _Variant>
    struct variant_alternative<_Np, const _Variant>
    { using type = const variant_alternative_t<_Np, _Variant>; };

  template<size_t _Np, typename _Variant>
    struct variant_alternative<_Np, volatile _Variant>
    { using type = volatile variant_alternative_t<_Np, _Variant>; };

  template<size_t _Np, typename _Variant>
    struct variant_alternative<_Np, const volatile _Variant>
    { using type = const volatile variant_alternative_t<_Np, _Variant>; };

  inline constexpr size_t variant_npos = -1;

  template<size_t _Np, typename... _Types>
    constexpr variant_alternative_t<_Np, variant<_Types...>>&
    get(variant<_Types...>&);

  template<size_t _Np, typename... _Types>
    constexpr variant_alternative_t<_Np, variant<_Types...>>&&
    get(variant<_Types...>&&);

  template<size_t _Np, typename... _Types>
    constexpr variant_alternative_t<_Np, variant<_Types...>> const&
    get(const variant<_Types...>&);

  template<size_t _Np, typename... _Types>
    constexpr variant_alternative_t<_Np, variant<_Types...>> const&&
    get(const variant<_Types...>&&);

  template<typename _Result_type, typename _Visitor, typename... _Variants>
    constexpr decltype(auto)
    __do_visit(_Visitor&& __visitor, _Variants&&... __variants);

  template <typename... _Types, typename _Tp>
   
    decltype(auto)
    __variant_cast(_Tp&& __rhs)
    {
      if constexpr (is_lvalue_reference_v<_Tp>)
 {
   if constexpr (is_const_v<remove_reference_t<_Tp>>)
     return static_cast<const variant<_Types...>&>(__rhs);
   else
     return static_cast<variant<_Types...>&>(__rhs);
 }
      else
        return static_cast<variant<_Types...>&&>(__rhs);
    }

namespace __detail
{
namespace __variant
{

  struct __variant_cookie {};

  struct __variant_idx_cookie { using type = __variant_idx_cookie; };

  template<typename _Tp> struct __deduce_visit_result { using type = _Tp; };


  template<typename _Visitor, typename... _Variants>
    constexpr void
    __raw_visit(_Visitor&& __visitor, _Variants&&... __variants)
    {
      std::__do_visit<__variant_cookie>(std::forward<_Visitor>(__visitor),
            std::forward<_Variants>(__variants)...);
    }


  template<typename _Visitor, typename... _Variants>
    constexpr void
    __raw_idx_visit(_Visitor&& __visitor, _Variants&&... __variants)
    {
      std::__do_visit<__variant_idx_cookie>(std::forward<_Visitor>(__visitor),
   std::forward<_Variants>(__variants)...);
    }



  template<typename... _Types>
    constexpr std::variant<_Types...>&
    __as(std::variant<_Types...>& __v) noexcept
    { return __v; }

  template<typename... _Types>
    constexpr const std::variant<_Types...>&
    __as(const std::variant<_Types...>& __v) noexcept
    { return __v; }

  template<typename... _Types>
    constexpr std::variant<_Types...>&&
    __as(std::variant<_Types...>&& __v) noexcept
    { return std::move(__v); }

  template<typename... _Types>
    constexpr const std::variant<_Types...>&&
    __as(const std::variant<_Types...>&& __v) noexcept
    { return std::move(__v); }


  template<typename _Type, bool = std::is_trivially_destructible_v<_Type>>
    struct _Uninitialized;







  template<typename _Type, bool>
    struct _Uninitialized
    {
      template<typename... _Args>
 constexpr
 _Uninitialized(in_place_index_t<0>, _Args&&... __args)
 : _M_storage(std::forward<_Args>(__args)...)
 { }


      constexpr const _Type& _M_get() const & noexcept
      { return _M_storage; }

      constexpr _Type& _M_get() & noexcept
      { return _M_storage; }

      constexpr const _Type&& _M_get() const && noexcept
      { return std::move(_M_storage); }

      constexpr _Type&& _M_get() && noexcept
      { return std::move(_M_storage); }


      _Type _M_storage;
    };





  template<typename _Type>
    struct _Uninitialized<_Type, false>
    {
      template<typename... _Args>
 constexpr
 _Uninitialized(in_place_index_t<0>, _Args&&... __args)
 {
   ::new ((void*)std::addressof(_M_storage))
     _Type(std::forward<_Args>(__args)...);
 }

      const _Type& _M_get() const & noexcept
      { return *_M_storage._M_ptr(); }

      _Type& _M_get() & noexcept
      { return *_M_storage._M_ptr(); }

      const _Type&& _M_get() const && noexcept
      { return std::move(*_M_storage._M_ptr()); }

      _Type&& _M_get() && noexcept
      { return std::move(*_M_storage._M_ptr()); }

      __gnu_cxx::__aligned_membuf<_Type> _M_storage;
    };

  template<size_t _Np, typename _Union>
    constexpr decltype(auto)
    __get_n(_Union&& __u) noexcept
    {
      if constexpr (_Np == 0)
 return std::forward<_Union>(__u)._M_first._M_get();
      else if constexpr (_Np == 1)
 return std::forward<_Union>(__u)._M_rest._M_first._M_get();
      else if constexpr (_Np == 2)
 return std::forward<_Union>(__u)._M_rest._M_rest._M_first._M_get();
      else
 return __variant::__get_n<_Np - 3>(
   std::forward<_Union>(__u)._M_rest._M_rest._M_rest);
    }
# 311 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/variant" 3
  template<size_t _Np, typename _Variant>
    constexpr decltype(auto)
    __get(_Variant&& __v) noexcept
    { return __variant::__get_n<_Np>(std::forward<_Variant>(__v)._M_u); }


  template<size_t _Np, typename _Union>
    constexpr decltype(auto)
    __construct_n(_Union& __u) noexcept
    {
      if constexpr (_Np == 0)
 return &__u._M_first;
      else if constexpr (_Np == 1)
 {
   std::_Construct(&__u._M_rest);
   return &__u._M_rest._M_first;
 }
      else if constexpr (_Np == 2)
 {
   std::_Construct(&__u._M_rest);
   std::_Construct(&__u._M_rest._M_rest);
   return &__u._M_rest._M_rest._M_first;
 }
      else
 {
   std::_Construct(&__u._M_rest);
   std::_Construct(&__u._M_rest._M_rest);
   std::_Construct(&__u._M_rest._M_rest._M_rest);
   return __variant::__construct_n<_Np - 3>(__u._M_rest._M_rest._M_rest);
 }
    }

  template<typename... _Types>
    struct _Traits
    {
      static constexpr bool _S_default_ctor =
   is_default_constructible_v<typename _Nth_type<0, _Types...>::type>;
      static constexpr bool _S_copy_ctor =
   (is_copy_constructible_v<_Types> && ...);
      static constexpr bool _S_move_ctor =
   (is_move_constructible_v<_Types> && ...);
      static constexpr bool _S_copy_assign =
   _S_copy_ctor
   && (is_copy_assignable_v<_Types> && ...);
      static constexpr bool _S_move_assign =
   _S_move_ctor
   && (is_move_assignable_v<_Types> && ...);

      static constexpr bool _S_trivial_dtor =
   (is_trivially_destructible_v<_Types> && ...);
      static constexpr bool _S_trivial_copy_ctor =
   (is_trivially_copy_constructible_v<_Types> && ...);
      static constexpr bool _S_trivial_move_ctor =
   (is_trivially_move_constructible_v<_Types> && ...);
      static constexpr bool _S_trivial_copy_assign =
   _S_trivial_dtor && _S_trivial_copy_ctor
   && (is_trivially_copy_assignable_v<_Types> && ...);
      static constexpr bool _S_trivial_move_assign =
   _S_trivial_dtor && _S_trivial_move_ctor
   && (is_trivially_move_assignable_v<_Types> && ...);



      static constexpr bool _S_nothrow_default_ctor =
   is_nothrow_default_constructible_v<
       typename _Nth_type<0, _Types...>::type>;
      static constexpr bool _S_nothrow_copy_ctor = false;
      static constexpr bool _S_nothrow_move_ctor =
   (is_nothrow_move_constructible_v<_Types> && ...);
      static constexpr bool _S_nothrow_copy_assign = false;
      static constexpr bool _S_nothrow_move_assign =
   _S_nothrow_move_ctor
   && (is_nothrow_move_assignable_v<_Types> && ...);
    };


  template<bool __trivially_destructible, typename... _Types>
    union _Variadic_union
    {
      _Variadic_union() = default;

      template<size_t _Np, typename... _Args>
 _Variadic_union(in_place_index_t<_Np>, _Args&&...) = delete;
    };

  template<bool __trivially_destructible, typename _First, typename... _Rest>
    union _Variadic_union<__trivially_destructible, _First, _Rest...>
    {
      constexpr _Variadic_union() : _M_rest() { }

      template<typename... _Args>
 constexpr
 _Variadic_union(in_place_index_t<0>, _Args&&... __args)
 : _M_first(in_place_index<0>, std::forward<_Args>(__args)...)
 { }

      template<size_t _Np, typename... _Args>
 constexpr
 _Variadic_union(in_place_index_t<_Np>, _Args&&... __args)
 : _M_rest(in_place_index<_Np-1>, std::forward<_Args>(__args)...)
 { }
# 429 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/variant" 3
      _Uninitialized<_First> _M_first;
      _Variadic_union<__trivially_destructible, _Rest...> _M_rest;
    };






  template<typename _Tp>
    struct _Never_valueless_alt
    : __and_<bool_constant<sizeof(_Tp) <= 256>, is_trivially_copyable<_Tp>>
    { };
# 453 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/variant" 3
  template <typename... _Types>
    constexpr bool __never_valueless()
    {
      return _Traits<_Types...>::_S_move_assign
 && (_Never_valueless_alt<_Types>::value && ...);
    }


  template<bool __trivially_destructible, typename... _Types>
    struct _Variant_storage;

  template <typename... _Types>
    using __select_index =
      typename __select_int::_Select_int_base<sizeof...(_Types),
           unsigned char,
           unsigned short>::type::value_type;

  template<typename... _Types>
    struct _Variant_storage<false, _Types...>
    {
      constexpr
      _Variant_storage()
      : _M_index(static_cast<__index_type>(variant_npos))
      { }

      template<size_t _Np, typename... _Args>
 constexpr
 _Variant_storage(in_place_index_t<_Np>, _Args&&... __args)
 : _M_u(in_place_index<_Np>, std::forward<_Args>(__args)...),
   _M_index{_Np}
 { }

      constexpr void
      _M_reset()
      {
 if (!_M_valid()) [[__unlikely__]]
   return;

 std::__do_visit<void>([](auto&& __this_mem) mutable
   {
     std::_Destroy(std::__addressof(__this_mem));
   }, __variant_cast<_Types...>(*this));

 _M_index = static_cast<__index_type>(variant_npos);
      }

     
      ~_Variant_storage()
      { _M_reset(); }

      constexpr bool
      _M_valid() const noexcept
      {
 if constexpr (__variant::__never_valueless<_Types...>())
   return true;
 return this->_M_index != __index_type(variant_npos);
      }

      _Variadic_union<false, _Types...> _M_u;
      using __index_type = __select_index<_Types...>;
      __index_type _M_index;
    };

  template<typename... _Types>
    struct _Variant_storage<true, _Types...>
    {
      constexpr
      _Variant_storage()
      : _M_index(static_cast<__index_type>(variant_npos))
      { }

      template<size_t _Np, typename... _Args>
 constexpr
 _Variant_storage(in_place_index_t<_Np>, _Args&&... __args)
 : _M_u(in_place_index<_Np>, std::forward<_Args>(__args)...),
   _M_index{_Np}
 { }

      constexpr void
      _M_reset() noexcept
      { _M_index = static_cast<__index_type>(variant_npos); }

      constexpr bool
      _M_valid() const noexcept
      {
 if constexpr (__variant::__never_valueless<_Types...>())
   return true;






 return this->_M_index != static_cast<__index_type>(variant_npos);
      }

      _Variadic_union<true, _Types...> _M_u;
      using __index_type = __select_index<_Types...>;
      __index_type _M_index;
    };


  template<size_t _Np, bool _Triv, typename... _Types, typename... _Args>
   
    inline void
    __emplace(_Variant_storage<_Triv, _Types...>& __v, _Args&&... __args)
    {
      __v._M_reset();
      auto* __addr = __variant::__construct_n<_Np>(__v._M_u);
      std::_Construct(__addr, in_place_index<0>,
        std::forward<_Args>(__args)...);

      __v._M_index = _Np;
    }

  template<typename... _Types>
    using _Variant_storage_alias =
 _Variant_storage<_Traits<_Types...>::_S_trivial_dtor, _Types...>;




  template<bool, typename... _Types>
    struct _Copy_ctor_base : _Variant_storage_alias<_Types...>
    {
      using _Base = _Variant_storage_alias<_Types...>;
      using _Base::_Base;

     
      _Copy_ctor_base(const _Copy_ctor_base& __rhs)
   noexcept(_Traits<_Types...>::_S_nothrow_copy_ctor)
      {
 __variant::__raw_idx_visit(
   [this](auto&& __rhs_mem, auto __rhs_index) mutable
   {
     constexpr size_t __j = __rhs_index;
     if constexpr (__j != variant_npos)
       std::_Construct(std::__addressof(this->_M_u),
         in_place_index<__j>, __rhs_mem);
   }, __variant_cast<_Types...>(__rhs));
 this->_M_index = __rhs._M_index;
      }

      _Copy_ctor_base(_Copy_ctor_base&&) = default;
      _Copy_ctor_base& operator=(const _Copy_ctor_base&) = default;
      _Copy_ctor_base& operator=(_Copy_ctor_base&&) = default;
    };

  template<typename... _Types>
    struct _Copy_ctor_base<true, _Types...> : _Variant_storage_alias<_Types...>
    {
      using _Base = _Variant_storage_alias<_Types...>;
      using _Base::_Base;
    };

  template<typename... _Types>
    using _Copy_ctor_alias =
 _Copy_ctor_base<_Traits<_Types...>::_S_trivial_copy_ctor, _Types...>;

  template<bool, typename... _Types>
    struct _Move_ctor_base : _Copy_ctor_alias<_Types...>
    {
      using _Base = _Copy_ctor_alias<_Types...>;
      using _Base::_Base;

     
      _Move_ctor_base(_Move_ctor_base&& __rhs)
   noexcept(_Traits<_Types...>::_S_nothrow_move_ctor)
      {
 __variant::__raw_idx_visit(
   [this](auto&& __rhs_mem, auto __rhs_index) mutable
   {
     constexpr size_t __j = __rhs_index;
     if constexpr (__j != variant_npos)
       std::_Construct(std::__addressof(this->_M_u),
         in_place_index<__j>,
         std::forward<decltype(__rhs_mem)>(__rhs_mem));
   }, __variant_cast<_Types...>(std::move(__rhs)));
 this->_M_index = __rhs._M_index;
      }

      _Move_ctor_base(const _Move_ctor_base&) = default;
      _Move_ctor_base& operator=(const _Move_ctor_base&) = default;
      _Move_ctor_base& operator=(_Move_ctor_base&&) = default;
    };

  template<typename... _Types>
    struct _Move_ctor_base<true, _Types...> : _Copy_ctor_alias<_Types...>
    {
      using _Base = _Copy_ctor_alias<_Types...>;
      using _Base::_Base;
    };

  template<typename... _Types>
    using _Move_ctor_alias =
 _Move_ctor_base<_Traits<_Types...>::_S_trivial_move_ctor, _Types...>;

  template<bool, typename... _Types>
    struct _Copy_assign_base : _Move_ctor_alias<_Types...>
    {
      using _Base = _Move_ctor_alias<_Types...>;
      using _Base::_Base;

     
      _Copy_assign_base&
      operator=(const _Copy_assign_base& __rhs)
   noexcept(_Traits<_Types...>::_S_nothrow_copy_assign)
      {
 __variant::__raw_idx_visit(
   [this](auto&& __rhs_mem, auto __rhs_index) mutable
   {
     constexpr size_t __j = __rhs_index;
     if constexpr (__j == variant_npos)
       this->_M_reset();
     else if (this->_M_index == __j)
       __variant::__get<__j>(*this) = __rhs_mem;
     else
       {
  using _Tj = typename _Nth_type<__j, _Types...>::type;
  if constexpr (is_nothrow_copy_constructible_v<_Tj>
         || !is_nothrow_move_constructible_v<_Tj>)
    __variant::__emplace<__j>(*this, __rhs_mem);
  else
    {
      using _Variant = variant<_Types...>;
      _Variant& __self = __variant_cast<_Types...>(*this);
      __self = _Variant(in_place_index<__j>, __rhs_mem);
    }
       }
   }, __variant_cast<_Types...>(__rhs));
 return *this;
      }

      _Copy_assign_base(const _Copy_assign_base&) = default;
      _Copy_assign_base(_Copy_assign_base&&) = default;
      _Copy_assign_base& operator=(_Copy_assign_base&&) = default;
    };

  template<typename... _Types>
    struct _Copy_assign_base<true, _Types...> : _Move_ctor_alias<_Types...>
    {
      using _Base = _Move_ctor_alias<_Types...>;
      using _Base::_Base;
    };

  template<typename... _Types>
    using _Copy_assign_alias =
      _Copy_assign_base<_Traits<_Types...>::_S_trivial_copy_assign, _Types...>;

  template<bool, typename... _Types>
    struct _Move_assign_base : _Copy_assign_alias<_Types...>
    {
      using _Base = _Copy_assign_alias<_Types...>;
      using _Base::_Base;

     
      _Move_assign_base&
      operator=(_Move_assign_base&& __rhs)
   noexcept(_Traits<_Types...>::_S_nothrow_move_assign)
      {
 __variant::__raw_idx_visit(
   [this](auto&& __rhs_mem, auto __rhs_index) mutable
   {
     constexpr size_t __j = __rhs_index;
     if constexpr (__j != variant_npos)
       {
  if (this->_M_index == __j)
    __variant::__get<__j>(*this) = std::move(__rhs_mem);
  else
    {
      using _Tj = typename _Nth_type<__j, _Types...>::type;
      if constexpr (is_nothrow_move_constructible_v<_Tj>)
        __variant::__emplace<__j>(*this, std::move(__rhs_mem));
      else
        {
   using _Variant = variant<_Types...>;
   _Variant& __self = __variant_cast<_Types...>(*this);
   __self.template emplace<__j>(std::move(__rhs_mem));
        }
    }
       }
     else
       this->_M_reset();
   }, __variant_cast<_Types...>(__rhs));
 return *this;
      }

      _Move_assign_base(const _Move_assign_base&) = default;
      _Move_assign_base(_Move_assign_base&&) = default;
      _Move_assign_base& operator=(const _Move_assign_base&) = default;
    };

  template<typename... _Types>
    struct _Move_assign_base<true, _Types...> : _Copy_assign_alias<_Types...>
    {
      using _Base = _Copy_assign_alias<_Types...>;
      using _Base::_Base;
    };

  template<typename... _Types>
    using _Move_assign_alias =
      _Move_assign_base<_Traits<_Types...>::_S_trivial_move_assign, _Types...>;

  template<typename... _Types>
    struct _Variant_base : _Move_assign_alias<_Types...>
    {
      using _Base = _Move_assign_alias<_Types...>;

      constexpr
      _Variant_base() noexcept(_Traits<_Types...>::_S_nothrow_default_ctor)
      : _Variant_base(in_place_index<0>) { }

      template<size_t _Np, typename... _Args>
 constexpr explicit
 _Variant_base(in_place_index_t<_Np> __i, _Args&&... __args)
 : _Base(__i, std::forward<_Args>(__args)...)
 { }

      _Variant_base(const _Variant_base&) = default;
      _Variant_base(_Variant_base&&) = default;
      _Variant_base& operator=(const _Variant_base&) = default;
      _Variant_base& operator=(_Variant_base&&) = default;
    };

  template<typename _Tp, typename... _Types>
    inline constexpr bool __exactly_once
      = std::__find_uniq_type_in_pack<_Tp, _Types...>() < sizeof...(_Types);


  template<typename _Ti> struct _Arr { _Ti _M_x[1]; };


  template<size_t _Ind, typename _Tp, typename _Ti, typename = void>
    struct _Build_FUN
    {


      void _S_fun() = delete;
    };


  template<size_t _Ind, typename _Tp, typename _Ti>
    struct _Build_FUN<_Ind, _Tp, _Ti,
        void_t<decltype(_Arr<_Ti>{{std::declval<_Tp>()}})>>
    {

      static integral_constant<size_t, _Ind> _S_fun(_Ti);
    };

  template<typename _Tp, typename _Variant,
    typename = make_index_sequence<variant_size_v<_Variant>>>
    struct _Build_FUNs;

  template<typename _Tp, typename... _Ti, size_t... _Ind>
    struct _Build_FUNs<_Tp, variant<_Ti...>, index_sequence<_Ind...>>
    : _Build_FUN<_Ind, _Tp, _Ti>...
    {
      using _Build_FUN<_Ind, _Tp, _Ti>::_S_fun...;
    };



  template<typename _Tp, typename _Variant>
    using _FUN_type
      = decltype(_Build_FUNs<_Tp, _Variant>::_S_fun(std::declval<_Tp>()));


  template<typename _Tp, typename _Variant, typename = void>
    inline constexpr size_t
    __accepted_index = variant_npos;

  template<typename _Tp, typename _Variant>
    inline constexpr size_t
    __accepted_index<_Tp, _Variant, void_t<_FUN_type<_Tp, _Variant>>>
      = _FUN_type<_Tp, _Variant>::value;

  template<typename _Maybe_variant_cookie, typename _Variant,
    typename = __remove_cvref_t<_Variant>>
    inline constexpr bool
    __extra_visit_slot_needed = false;

  template<typename _Var, typename... _Types>
    inline constexpr bool
    __extra_visit_slot_needed<__variant_cookie, _Var, variant<_Types...>>
      = !__variant::__never_valueless<_Types...>();

  template<typename _Var, typename... _Types>
    inline constexpr bool
    __extra_visit_slot_needed<__variant_idx_cookie, _Var, variant<_Types...>>
      = !__variant::__never_valueless<_Types...>();


  template<typename _Tp, size_t... _Dimensions>
    struct _Multi_array;


  template<typename _Tp>
    struct _Multi_array<_Tp>
    {
      template<typename>
 struct __untag_result
 : false_type
 { using element_type = _Tp; };

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wignored-qualifiers"
      template <typename... _Args>
 struct __untag_result<const void(*)(_Args...)>
 : false_type
 { using element_type = void(*)(_Args...); };
#pragma GCC diagnostic pop

      template <typename... _Args>
 struct __untag_result<__variant_cookie(*)(_Args...)>
 : false_type
 { using element_type = void(*)(_Args...); };

      template <typename... _Args>
 struct __untag_result<__variant_idx_cookie(*)(_Args...)>
 : false_type
 { using element_type = void(*)(_Args...); };

      template <typename _Res, typename... _Args>
 struct __untag_result<__deduce_visit_result<_Res>(*)(_Args...)>
 : true_type
 { using element_type = _Res(*)(_Args...); };

      using __result_is_deduced = __untag_result<_Tp>;

      constexpr const typename __untag_result<_Tp>::element_type&
      _M_access() const
      { return _M_data; }

      typename __untag_result<_Tp>::element_type _M_data;
    };


  template<typename _Ret,
    typename _Visitor,
    typename... _Variants,
    size_t __first, size_t... __rest>
    struct _Multi_array<_Ret(*)(_Visitor, _Variants...), __first, __rest...>
    {
      static constexpr size_t __index =
 sizeof...(_Variants) - sizeof...(__rest) - 1;

      using _Variant = typename _Nth_type<__index, _Variants...>::type;

      static constexpr int __do_cookie =
 __extra_visit_slot_needed<_Ret, _Variant> ? 1 : 0;

      using _Tp = _Ret(*)(_Visitor, _Variants...);

      template<typename... _Args>
 constexpr decltype(auto)
 _M_access(size_t __first_index, _Args... __rest_indices) const
        {
   return _M_arr[__first_index + __do_cookie]
     ._M_access(__rest_indices...);
 }

      _Multi_array<_Tp, __rest...> _M_arr[__first + __do_cookie];
    };
# 944 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/variant" 3
  template<typename _Array_type, typename _Index_seq>
    struct __gen_vtable_impl;
# 954 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/variant" 3
  template<typename _Result_type, typename _Visitor, size_t... __dimensions,
    typename... _Variants, size_t... __indices>
    struct __gen_vtable_impl<
 _Multi_array<_Result_type (*)(_Visitor, _Variants...), __dimensions...>,
 std::index_sequence<__indices...>>
    {
      using _Next =
   remove_reference_t<typename _Nth_type<sizeof...(__indices),
        _Variants...>::type>;
      using _Array_type =
   _Multi_array<_Result_type (*)(_Visitor, _Variants...),
         __dimensions...>;

      static constexpr _Array_type
      _S_apply()
      {
 _Array_type __vtable{};
 _S_apply_all_alts(
   __vtable, make_index_sequence<variant_size_v<_Next>>());
 return __vtable;
      }

      template<size_t... __var_indices>
 static constexpr void
 _S_apply_all_alts(_Array_type& __vtable,
     std::index_sequence<__var_indices...>)
 {
   if constexpr (__extra_visit_slot_needed<_Result_type, _Next>)
     (_S_apply_single_alt<true, __var_indices>(
       __vtable._M_arr[__var_indices + 1],
       &(__vtable._M_arr[0])), ...);
   else
     (_S_apply_single_alt<false, __var_indices>(
       __vtable._M_arr[__var_indices]), ...);
 }

      template<bool __do_cookie, size_t __index, typename _Tp>
 static constexpr void
 _S_apply_single_alt(_Tp& __element, _Tp* __cookie_element = nullptr)
 {
   if constexpr (__do_cookie)
     {
       __element = __gen_vtable_impl<
  _Tp,
  std::index_sequence<__indices..., __index>>::_S_apply();
       *__cookie_element = __gen_vtable_impl<
  _Tp,
  std::index_sequence<__indices..., variant_npos>>::_S_apply();
     }
   else
     {
       auto __tmp_element = __gen_vtable_impl<
  remove_reference_t<decltype(__element)>,
  std::index_sequence<__indices..., __index>>::_S_apply();
       static_assert(is_same_v<_Tp, decltype(__tmp_element)>,
       "std::visit requires the visitor to have the same "
       "return type for all alternatives of a variant");
       __element = __tmp_element;
     }
 }
    };




  template<typename _Result_type, typename _Visitor, typename... _Variants,
    size_t... __indices>
    struct __gen_vtable_impl<
      _Multi_array<_Result_type (*)(_Visitor, _Variants...)>,
     std::index_sequence<__indices...>>
    {
      using _Array_type =
   _Multi_array<_Result_type (*)(_Visitor, _Variants...)>;

      template<size_t __index, typename _Variant>
 static constexpr decltype(auto)
 __element_by_index_or_cookie(_Variant&& __var) noexcept
        {
   if constexpr (__index != variant_npos)
     return __variant::__get<__index>(std::forward<_Variant>(__var));
   else
     return __variant_cookie{};
 }

      static constexpr decltype(auto)
      __visit_invoke(_Visitor&& __visitor, _Variants... __vars)
      {
 if constexpr (is_same_v<_Result_type, __variant_idx_cookie>)


   std::__invoke(std::forward<_Visitor>(__visitor),
       __element_by_index_or_cookie<__indices>(
  std::forward<_Variants>(__vars))...,
       integral_constant<size_t, __indices>()...);
 else if constexpr (is_same_v<_Result_type, __variant_cookie>)

   std::__invoke(std::forward<_Visitor>(__visitor),
       __element_by_index_or_cookie<__indices>(
  std::forward<_Variants>(__vars))...);
 else if constexpr (_Array_type::__result_is_deduced::value)

   return std::__invoke(std::forward<_Visitor>(__visitor),
       __element_by_index_or_cookie<__indices>(
  std::forward<_Variants>(__vars))...);
 else
   return std::__invoke_r<_Result_type>(
       std::forward<_Visitor>(__visitor),
       __variant::__get<__indices>(std::forward<_Variants>(__vars))...);
      }

      static constexpr auto
      _S_apply()
      {
 if constexpr (_Array_type::__result_is_deduced::value)
   {
     constexpr bool __visit_ret_type_mismatch =
       !is_same_v<typename _Result_type::type,
    decltype(__visit_invoke(std::declval<_Visitor>(),
        std::declval<_Variants>()...))>;
     if constexpr (__visit_ret_type_mismatch)
       {
  struct __cannot_match {};
  return __cannot_match{};
       }
     else
       return _Array_type{&__visit_invoke};
   }
 else
   return _Array_type{&__visit_invoke};
      }
    };

  template<typename _Result_type, typename _Visitor, typename... _Variants>
    struct __gen_vtable
    {
      using _Array_type =
   _Multi_array<_Result_type (*)(_Visitor, _Variants...),
         variant_size_v<remove_reference_t<_Variants>>...>;

      static constexpr _Array_type _S_vtable
 = __gen_vtable_impl<_Array_type, std::index_sequence<>>::_S_apply();
    };


  template<size_t _Nm, typename _Tp>
    struct _Base_dedup : public _Tp { };

  template<typename _Variant, typename __indices>
    struct _Variant_hash_base;

  template<typename... _Types, size_t... __indices>
    struct _Variant_hash_base<variant<_Types...>,
         std::index_sequence<__indices...>>
    : _Base_dedup<__indices, __hash_empty_base<remove_const_t<_Types>>>...
    { };



  template<size_t _Np, typename _Variant,
      typename _AsV = decltype(__variant::__as(std::declval<_Variant>())),
      typename _Tp = variant_alternative_t<_Np, remove_reference_t<_AsV>>>
    using __get_t
      = __conditional_t<is_lvalue_reference_v<_Variant>, _Tp&, _Tp&&>;


  template<typename _Visitor, typename... _Variants>
    using __visit_result_t
      = invoke_result_t<_Visitor, __get_t<0, _Variants>...>;

  template<typename _Tp, typename... _Types>
    constexpr inline bool __same_types = (is_same_v<_Tp, _Types> && ...);

  template <typename _Visitor, typename _Variant, size_t... _Idxs>
    constexpr bool __check_visitor_results(std::index_sequence<_Idxs...>)
    {
      return __same_types<
 invoke_result_t<_Visitor, __get_t<_Idxs, _Variant>>...
 >;
    }

}
}

  template<typename _Tp, typename... _Types>
    constexpr bool
    holds_alternative(const variant<_Types...>& __v) noexcept
    {
      static_assert(__detail::__variant::__exactly_once<_Tp, _Types...>,
      "T must occur exactly once in alternatives");
      return __v.index() == std::__find_uniq_type_in_pack<_Tp, _Types...>();
    }

  template<typename _Tp, typename... _Types>
    constexpr _Tp&
    get(variant<_Types...>& __v)
    {
      static_assert(__detail::__variant::__exactly_once<_Tp, _Types...>,
      "T must occur exactly once in alternatives");
      constexpr size_t __n = std::__find_uniq_type_in_pack<_Tp, _Types...>();
      return std::get<__n>(__v);
    }

  template<typename _Tp, typename... _Types>
    constexpr _Tp&&
    get(variant<_Types...>&& __v)
    {
      static_assert(__detail::__variant::__exactly_once<_Tp, _Types...>,
      "T must occur exactly once in alternatives");
      constexpr size_t __n = std::__find_uniq_type_in_pack<_Tp, _Types...>();
      return std::get<__n>(std::move(__v));
    }

  template<typename _Tp, typename... _Types>
    constexpr const _Tp&
    get(const variant<_Types...>& __v)
    {
      static_assert(__detail::__variant::__exactly_once<_Tp, _Types...>,
      "T must occur exactly once in alternatives");
      constexpr size_t __n = std::__find_uniq_type_in_pack<_Tp, _Types...>();
      return std::get<__n>(__v);
    }

  template<typename _Tp, typename... _Types>
    constexpr const _Tp&&
    get(const variant<_Types...>&& __v)
    {
      static_assert(__detail::__variant::__exactly_once<_Tp, _Types...>,
      "T must occur exactly once in alternatives");
      constexpr size_t __n = std::__find_uniq_type_in_pack<_Tp, _Types...>();
      return std::get<__n>(std::move(__v));
    }

  template<size_t _Np, typename... _Types>
    constexpr add_pointer_t<variant_alternative_t<_Np, variant<_Types...>>>
    get_if(variant<_Types...>* __ptr) noexcept
    {
      using _Alternative_type = variant_alternative_t<_Np, variant<_Types...>>;
      static_assert(_Np < sizeof...(_Types),
      "The index must be in [0, number of alternatives)");
      static_assert(!is_void_v<_Alternative_type>, "_Tp must not be void");
      if (__ptr && __ptr->index() == _Np)
 return std::addressof(__detail::__variant::__get<_Np>(*__ptr));
      return nullptr;
    }

  template<size_t _Np, typename... _Types>
    constexpr
    add_pointer_t<const variant_alternative_t<_Np, variant<_Types...>>>
    get_if(const variant<_Types...>* __ptr) noexcept
    {
      using _Alternative_type = variant_alternative_t<_Np, variant<_Types...>>;
      static_assert(_Np < sizeof...(_Types),
      "The index must be in [0, number of alternatives)");
      static_assert(!is_void_v<_Alternative_type>, "_Tp must not be void");
      if (__ptr && __ptr->index() == _Np)
 return std::addressof(__detail::__variant::__get<_Np>(*__ptr));
      return nullptr;
    }

  template<typename _Tp, typename... _Types>
    constexpr add_pointer_t<_Tp>
    get_if(variant<_Types...>* __ptr) noexcept
    {
      static_assert(__detail::__variant::__exactly_once<_Tp, _Types...>,
      "T must occur exactly once in alternatives");
      static_assert(!is_void_v<_Tp>, "_Tp must not be void");
      constexpr size_t __n = std::__find_uniq_type_in_pack<_Tp, _Types...>();
      return std::get_if<__n>(__ptr);
    }

  template<typename _Tp, typename... _Types>
    constexpr add_pointer_t<const _Tp>
    get_if(const variant<_Types...>* __ptr) noexcept
    {
      static_assert(__detail::__variant::__exactly_once<_Tp, _Types...>,
      "T must occur exactly once in alternatives");
      static_assert(!is_void_v<_Tp>, "_Tp must not be void");
      constexpr size_t __n = std::__find_uniq_type_in_pack<_Tp, _Types...>();
      return std::get_if<__n>(__ptr);
    }

namespace __detail::__variant
{
  template<typename _Ret, typename _Vp, typename _Op>
    constexpr _Ret
    __compare(_Ret __ret, const _Vp& __lhs, const _Vp& __rhs, _Op __op)
    {
      __variant::__raw_idx_visit(
 [&__ret, &__lhs, __op] (auto&& __rhs_mem, auto __rhs_index) mutable
 {
   if constexpr (__rhs_index != variant_npos)
     {
       if (__lhs.index() == __rhs_index.value)
  {
    auto& __this_mem = std::get<__rhs_index>(__lhs);
    __ret = __op(__this_mem, __rhs_mem);
    return;
  }
     }
   __ret = __op(__lhs.index() + 1, __rhs_index + 1);
 }, __rhs);
      return __ret;
    }
}

  template<typename... _Types>




    constexpr bool
    operator== [[nodiscard]] (const variant<_Types...>& __lhs,
         const variant<_Types...>& __rhs)
    {
      namespace __variant = __detail::__variant;
      return __variant::__compare(true, __lhs, __rhs,
      [](auto&& __l, auto&& __r) -> bool {
        return __l == __r;
      });
    }

  template<typename... _Types>




    constexpr bool
    operator!= [[nodiscard]] (const variant<_Types...>& __lhs,
         const variant<_Types...>& __rhs)
    {
      namespace __variant = __detail::__variant;
      return __variant::__compare(true, __lhs, __rhs,
      [](auto&& __l, auto&& __r) -> bool {
        return __l != __r;
      });
    }

  template<typename... _Types>




    constexpr bool
    operator< [[nodiscard]] (const variant<_Types...>& __lhs,
        const variant<_Types...>& __rhs)
    {
      namespace __variant = __detail::__variant;
      return __variant::__compare(true, __lhs, __rhs,
      [](auto&& __l, auto&& __r) -> bool {
        return __l < __r;
      });
    }

  template<typename... _Types>




    constexpr bool
    operator<= [[nodiscard]] (const variant<_Types...>& __lhs,
         const variant<_Types...>& __rhs)
    {
      namespace __variant = __detail::__variant;
      return __variant::__compare(true, __lhs, __rhs,
      [](auto&& __l, auto&& __r) -> bool {
        return __l <= __r;
      });
    }

  template<typename... _Types>




    constexpr bool
    operator> [[nodiscard]] (const variant<_Types...>& __lhs,
        const variant<_Types...>& __rhs)
    {
      namespace __variant = __detail::__variant;
      return __variant::__compare(true, __lhs, __rhs,
      [](auto&& __l, auto&& __r) -> bool {
        return __l > __r;
      });
    }

  template<typename... _Types>




    constexpr bool
    operator>= [[nodiscard]] (const variant<_Types...>& __lhs,
         const variant<_Types...>& __rhs)
    {
      namespace __variant = __detail::__variant;
      return __variant::__compare(true, __lhs, __rhs,
      [](auto&& __l, auto&& __r) -> bool {
        return __l >= __r;
      });
    }
# 1372 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/variant" 3
  template<typename _Visitor, typename... _Variants>
    constexpr __detail::__variant::__visit_result_t<_Visitor, _Variants...>
    visit(_Visitor&&, _Variants&&...);







  template<typename... _Types>
   
    inline enable_if_t<(is_move_constructible_v<_Types> && ...)
   && (is_swappable_v<_Types> && ...)>
    swap(variant<_Types...>& __lhs, variant<_Types...>& __rhs)
    noexcept(noexcept(__lhs.swap(__rhs)))
    { __lhs.swap(__rhs); }

  template<typename... _Types>
    enable_if_t<!((is_move_constructible_v<_Types> && ...)
     && (is_swappable_v<_Types> && ...))>
    swap(variant<_Types...>&, variant<_Types...>&) = delete;

  [[noreturn]] void __throw_bad_variant_access(unsigned);

  class bad_variant_access : public exception
  {
  public:
    bad_variant_access() noexcept { }

    const char* what() const noexcept override
    { return _M_reason; }

  private:

    bad_variant_access(const char* __reason) noexcept : _M_reason(__reason) { }


    const char* _M_reason = "bad variant access";

    friend void __throw_bad_variant_access([[maybe_unused]] unsigned __n)
    {
      [[maybe_unused]] static constexpr const char* __reasons[] = {
 "std::get: wrong index for variant",
 "std::get: variant is valueless",
 "std::visit: variant is valueless",
 "std::visit<R>: variant is valueless",
      };
      (throw (bad_variant_access(__reasons[__n % 4u])));
    }
  };

  template<typename... _Types>
    class variant
    : private __detail::__variant::_Variant_base<_Types...>,
      private _Enable_copy_move<
 __detail::__variant::_Traits<_Types...>::_S_copy_ctor,
 __detail::__variant::_Traits<_Types...>::_S_copy_assign,
 __detail::__variant::_Traits<_Types...>::_S_move_ctor,
 __detail::__variant::_Traits<_Types...>::_S_move_assign,
 variant<_Types...>>
    {
    private:
      template <typename... _UTypes, typename _Tp>
 friend decltype(auto)
 __variant_cast(_Tp&&);

      static_assert(sizeof...(_Types) > 0,
      "variant must have at least one alternative");
      static_assert(((std::is_object_v<_Types> && !is_array_v<_Types>) && ...),
      "variant alternatives must be non-array object types");

      using _Base = __detail::__variant::_Variant_base<_Types...>;

      template<typename _Tp>
 static constexpr bool __not_self
   = !is_same_v<__remove_cvref_t<_Tp>, variant>;

      template<typename _Tp>
 static constexpr bool
 __exactly_once = __detail::__variant::__exactly_once<_Tp, _Types...>;

      template<typename _Tp>
 static constexpr size_t __accepted_index
   = __detail::__variant::__accepted_index<_Tp, variant>;

      template<size_t _Np, typename = enable_if_t<(_Np < sizeof...(_Types))>>
 using __to_type = typename _Nth_type<_Np, _Types...>::type;

      template<typename _Tp, typename = enable_if_t<__not_self<_Tp>>>
 using __accepted_type = __to_type<__accepted_index<_Tp>>;

      template<typename _Tp>
 static constexpr size_t __index_of
   = std::__find_uniq_type_in_pack<_Tp, _Types...>();

      using _Traits = __detail::__variant::_Traits<_Types...>;

      template<typename _Tp>
 static constexpr bool __not_in_place_tag
   = !__is_in_place_type_v<__remove_cvref_t<_Tp>>
       && !__is_in_place_index_v<__remove_cvref_t<_Tp>>;

    public:



      template<typename _Tp0 = __to_type<0>,
        typename = enable_if_t<is_default_constructible_v<_Tp0>>>
 constexpr
 variant() noexcept(is_nothrow_default_constructible_v<__to_type<0>>)
 { }


      variant(const variant& __rhs) = default;
      variant(variant&&) = default;
      variant& operator=(const variant&) = default;
      variant& operator=(variant&&) = default;
      ~variant() = default;

      template<typename _Tp,
        typename = enable_if_t<sizeof...(_Types) != 0>,
        typename = enable_if_t<__not_in_place_tag<_Tp>>,
        typename _Tj = __accepted_type<_Tp&&>,
        typename = enable_if_t<__exactly_once<_Tj>
          && is_constructible_v<_Tj, _Tp>>>
 constexpr
 variant(_Tp&& __t)
 noexcept(is_nothrow_constructible_v<_Tj, _Tp>)
 : variant(in_place_index<__accepted_index<_Tp>>,
    std::forward<_Tp>(__t))
 { }

      template<typename _Tp, typename... _Args,
        typename = enable_if_t<__exactly_once<_Tp>
          && is_constructible_v<_Tp, _Args...>>>
 constexpr explicit
 variant(in_place_type_t<_Tp>, _Args&&... __args)
 : variant(in_place_index<__index_of<_Tp>>,
    std::forward<_Args>(__args)...)
 { }

      template<typename _Tp, typename _Up, typename... _Args,
        typename = enable_if_t<__exactly_once<_Tp>
          && is_constructible_v<_Tp,
        initializer_list<_Up>&, _Args...>>>
 constexpr explicit
 variant(in_place_type_t<_Tp>, initializer_list<_Up> __il,
  _Args&&... __args)
 : variant(in_place_index<__index_of<_Tp>>, __il,
    std::forward<_Args>(__args)...)
 { }

      template<size_t _Np, typename... _Args,
        typename _Tp = __to_type<_Np>,
        typename = enable_if_t<is_constructible_v<_Tp, _Args...>>>
 constexpr explicit
 variant(in_place_index_t<_Np>, _Args&&... __args)
 : _Base(in_place_index<_Np>, std::forward<_Args>(__args)...)
 { }

      template<size_t _Np, typename _Up, typename... _Args,
        typename _Tp = __to_type<_Np>,
        typename = enable_if_t<is_constructible_v<_Tp,
        initializer_list<_Up>&,
        _Args...>>>
 constexpr explicit
 variant(in_place_index_t<_Np>, initializer_list<_Up> __il,
  _Args&&... __args)
 : _Base(in_place_index<_Np>, __il, std::forward<_Args>(__args)...)
 { }

      template<typename _Tp>

 enable_if_t<__exactly_once<__accepted_type<_Tp&&>>
      && is_constructible_v<__accepted_type<_Tp&&>, _Tp>
      && is_assignable_v<__accepted_type<_Tp&&>&, _Tp>,
      variant&>
 operator=(_Tp&& __rhs)
 noexcept(is_nothrow_assignable_v<__accepted_type<_Tp&&>&, _Tp>
   && is_nothrow_constructible_v<__accepted_type<_Tp&&>, _Tp>)
 {
   constexpr auto __index = __accepted_index<_Tp>;
   if (index() == __index)
     std::get<__index>(*this) = std::forward<_Tp>(__rhs);
   else
     {
       using _Tj = __accepted_type<_Tp&&>;
       if constexpr (is_nothrow_constructible_v<_Tj, _Tp>
       || !is_nothrow_move_constructible_v<_Tj>)
  this->emplace<__index>(std::forward<_Tp>(__rhs));
       else


  this->emplace<__index>(_Tj(std::forward<_Tp>(__rhs)));
     }
   return *this;
 }

      template<typename _Tp, typename... _Args>

 enable_if_t<is_constructible_v<_Tp, _Args...> && __exactly_once<_Tp>,
      _Tp&>
 emplace(_Args&&... __args)
 {
   constexpr size_t __index = __index_of<_Tp>;
   return this->emplace<__index>(std::forward<_Args>(__args)...);
 }

      template<typename _Tp, typename _Up, typename... _Args>

 enable_if_t<is_constructible_v<_Tp, initializer_list<_Up>&, _Args...>
      && __exactly_once<_Tp>,
      _Tp&>
 emplace(initializer_list<_Up> __il, _Args&&... __args)
 {
   constexpr size_t __index = __index_of<_Tp>;
   return this->emplace<__index>(__il, std::forward<_Args>(__args)...);
 }

      template<size_t _Np, typename... _Args>

 enable_if_t<is_constructible_v<__to_type<_Np>, _Args...>,
      __to_type<_Np>&>
 emplace(_Args&&... __args)
 {
   namespace __variant = std::__detail::__variant;
   using type = typename _Nth_type<_Np, _Types...>::type;


   if constexpr (is_nothrow_constructible_v<type, _Args...>)
     {
       __variant::__emplace<_Np>(*this, std::forward<_Args>(__args)...);
     }
   else if constexpr (is_scalar_v<type>)
     {

       const type __tmp(std::forward<_Args>(__args)...);

       __variant::__emplace<_Np>(*this, __tmp);
     }
   else if constexpr (__variant::_Never_valueless_alt<type>()
       && _Traits::_S_move_assign)
     {

       variant __tmp(in_place_index<_Np>,
       std::forward<_Args>(__args)...);

       *this = std::move(__tmp);
     }
   else
     {


       __variant::__emplace<_Np>(*this, std::forward<_Args>(__args)...);
     }
   return std::get<_Np>(*this);
 }

      template<size_t _Np, typename _Up, typename... _Args>

 enable_if_t<is_constructible_v<__to_type<_Np>,
           initializer_list<_Up>&, _Args...>,
      __to_type<_Np>&>
 emplace(initializer_list<_Up> __il, _Args&&... __args)
 {
   namespace __variant = std::__detail::__variant;
   using type = typename _Nth_type<_Np, _Types...>::type;


   if constexpr (is_nothrow_constructible_v<type,
         initializer_list<_Up>&,
         _Args...>)
     {
       __variant::__emplace<_Np>(*this, __il,
     std::forward<_Args>(__args)...);
     }
   else if constexpr (__variant::_Never_valueless_alt<type>()
       && _Traits::_S_move_assign)
     {

       variant __tmp(in_place_index<_Np>, __il,
       std::forward<_Args>(__args)...);

       *this = std::move(__tmp);
     }
   else
     {


       __variant::__emplace<_Np>(*this, __il,
     std::forward<_Args>(__args)...);
     }
   return std::get<_Np>(*this);
 }

      template<size_t _Np, typename... _Args>
 enable_if_t<!(_Np < sizeof...(_Types))> emplace(_Args&&...) = delete;

      template<typename _Tp, typename... _Args>
 enable_if_t<!__exactly_once<_Tp>> emplace(_Args&&...) = delete;

      constexpr bool valueless_by_exception() const noexcept
      { return !this->_M_valid(); }

      constexpr size_t index() const noexcept
      {
 using __index_type = typename _Base::__index_type;
 if constexpr (__detail::__variant::__never_valueless<_Types...>())
   return this->_M_index;
 else if constexpr (sizeof...(_Types) <= __index_type(-1) / 2)
   return make_signed_t<__index_type>(this->_M_index);
 else
   return size_t(__index_type(this->_M_index + 1)) - 1;
      }

     
      void
      swap(variant& __rhs)
      noexcept((__is_nothrow_swappable<_Types>::value && ...)
        && is_nothrow_move_constructible_v<variant>)
      {
 static_assert((is_move_constructible_v<_Types> && ...));


 if (__rhs.valueless_by_exception()) [[__unlikely__]]
   {
     if (!this->valueless_by_exception()) [[__likely__]]
       __rhs.swap(*this);
     return;
   }

 namespace __variant = __detail::__variant;

 __variant::__raw_idx_visit(
   [this, &__rhs](auto&& __rhs_mem, auto __rhs_index) mutable
   {
     constexpr size_t __j = __rhs_index;
     if constexpr (__j != variant_npos)
       {
  if (this->index() == __j)
    {
      using std::swap;
      swap(std::get<__j>(*this), __rhs_mem);
    }
  else
    {
      auto __tmp(std::move(__rhs_mem));

      if constexpr (_Traits::_S_trivial_move_assign)
        __rhs = std::move(*this);
      else
        __variant::__raw_idx_visit(
   [&__rhs](auto&& __this_mem, auto __this_index) mutable
   {
     constexpr size_t __k = __this_index;
     if constexpr (__k != variant_npos)
       __variant::__emplace<__k>(__rhs,
            std::move(__this_mem));
   }, *this);

      __variant::__emplace<__j>(*this, std::move(__tmp));
    }
       }
   }, __rhs);
      }
# 1779 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/variant" 3
    private:
      template<size_t _Np, typename _Vp>
 friend constexpr decltype(auto)
 __detail::__variant::__get(_Vp&& __v) noexcept;
    };

  template<size_t _Np, typename... _Types>
    constexpr variant_alternative_t<_Np, variant<_Types...>>&
    get(variant<_Types...>& __v)
    {
      static_assert(_Np < sizeof...(_Types),
      "The index must be in [0, number of alternatives)");
      if (__v.index() != _Np)
 __throw_bad_variant_access(__v.valueless_by_exception());
      return __detail::__variant::__get<_Np>(__v);
    }

  template<size_t _Np, typename... _Types>
    constexpr variant_alternative_t<_Np, variant<_Types...>>&&
    get(variant<_Types...>&& __v)
    {
      static_assert(_Np < sizeof...(_Types),
      "The index must be in [0, number of alternatives)");
      if (__v.index() != _Np)
 __throw_bad_variant_access(__v.valueless_by_exception());
      return __detail::__variant::__get<_Np>(std::move(__v));
    }

  template<size_t _Np, typename... _Types>
    constexpr const variant_alternative_t<_Np, variant<_Types...>>&
    get(const variant<_Types...>& __v)
    {
      static_assert(_Np < sizeof...(_Types),
      "The index must be in [0, number of alternatives)");
      if (__v.index() != _Np)
 __throw_bad_variant_access(__v.valueless_by_exception());
      return __detail::__variant::__get<_Np>(__v);
    }

  template<size_t _Np, typename... _Types>
    constexpr const variant_alternative_t<_Np, variant<_Types...>>&&
    get(const variant<_Types...>&& __v)
    {
      static_assert(_Np < sizeof...(_Types),
      "The index must be in [0, number of alternatives)");
      if (__v.index() != _Np)
 __throw_bad_variant_access(__v.valueless_by_exception());
      return __detail::__variant::__get<_Np>(std::move(__v));
    }


  template<typename _Result_type, typename _Visitor, typename... _Variants>
    constexpr decltype(auto)
    __do_visit(_Visitor&& __visitor, _Variants&&... __variants)
    {

      if constexpr (sizeof...(_Variants) == 0)
 {
   if constexpr (is_void_v<_Result_type>)
     return (void) std::forward<_Visitor>(__visitor)();
   else
     return std::forward<_Visitor>(__visitor)();
 }
      else
 {
   constexpr size_t __max = 11;


   using _V0 = typename _Nth_type<0, _Variants...>::type;

   constexpr auto __n = variant_size_v<remove_reference_t<_V0>>;

   if constexpr (sizeof...(_Variants) > 1 || __n > __max)
     {

       constexpr auto& __vtable = __detail::__variant::__gen_vtable<
  _Result_type, _Visitor&&, _Variants&&...>::_S_vtable;

       auto __func_ptr = __vtable._M_access(__variants.index()...);
       return (*__func_ptr)(std::forward<_Visitor>(__visitor),
       std::forward<_Variants>(__variants)...);
     }
   else
     {

       _V0& __v0
  = [](_V0& __v, ...) -> _V0& { return __v; }(__variants...);

       using __detail::__variant::_Multi_array;
       using __detail::__variant::__gen_vtable_impl;
       using _Ma = _Multi_array<_Result_type (*)(_Visitor&&, _V0&&)>;
# 1889 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/variant" 3
       switch (__v0.index())
  {
    case 0: { if constexpr (0 < __n) { return __gen_vtable_impl<_Ma, index_sequence<0>>:: __visit_invoke(std::forward<_Visitor>(__visitor), std::forward<_V0>(__v0)); } else __builtin_unreachable(); }
    case 1: { if constexpr (1 < __n) { return __gen_vtable_impl<_Ma, index_sequence<1>>:: __visit_invoke(std::forward<_Visitor>(__visitor), std::forward<_V0>(__v0)); } else __builtin_unreachable(); }
    case 2: { if constexpr (2 < __n) { return __gen_vtable_impl<_Ma, index_sequence<2>>:: __visit_invoke(std::forward<_Visitor>(__visitor), std::forward<_V0>(__v0)); } else __builtin_unreachable(); }
    case 3: { if constexpr (3 < __n) { return __gen_vtable_impl<_Ma, index_sequence<3>>:: __visit_invoke(std::forward<_Visitor>(__visitor), std::forward<_V0>(__v0)); } else __builtin_unreachable(); }
    case 4: { if constexpr (4 < __n) { return __gen_vtable_impl<_Ma, index_sequence<4>>:: __visit_invoke(std::forward<_Visitor>(__visitor), std::forward<_V0>(__v0)); } else __builtin_unreachable(); }
    case 5: { if constexpr (5 < __n) { return __gen_vtable_impl<_Ma, index_sequence<5>>:: __visit_invoke(std::forward<_Visitor>(__visitor), std::forward<_V0>(__v0)); } else __builtin_unreachable(); }
    case 6: { if constexpr (6 < __n) { return __gen_vtable_impl<_Ma, index_sequence<6>>:: __visit_invoke(std::forward<_Visitor>(__visitor), std::forward<_V0>(__v0)); } else __builtin_unreachable(); }
    case 7: { if constexpr (7 < __n) { return __gen_vtable_impl<_Ma, index_sequence<7>>:: __visit_invoke(std::forward<_Visitor>(__visitor), std::forward<_V0>(__v0)); } else __builtin_unreachable(); }
    case 8: { if constexpr (8 < __n) { return __gen_vtable_impl<_Ma, index_sequence<8>>:: __visit_invoke(std::forward<_Visitor>(__visitor), std::forward<_V0>(__v0)); } else __builtin_unreachable(); }
    case 9: { if constexpr (9 < __n) { return __gen_vtable_impl<_Ma, index_sequence<9>>:: __visit_invoke(std::forward<_Visitor>(__visitor), std::forward<_V0>(__v0)); } else __builtin_unreachable(); }
    case 10: { if constexpr (10 < __n) { return __gen_vtable_impl<_Ma, index_sequence<10>>:: __visit_invoke(std::forward<_Visitor>(__visitor), std::forward<_V0>(__v0)); } else __builtin_unreachable(); }
  case variant_npos:
    using __detail::__variant::__variant_idx_cookie;
    using __detail::__variant::__variant_cookie;
    if constexpr (is_same_v<_Result_type, __variant_idx_cookie>
    || is_same_v<_Result_type, __variant_cookie>)
      {
        using _Npos = index_sequence<variant_npos>;
        return __gen_vtable_impl<_Ma, _Npos>::
   __visit_invoke(std::forward<_Visitor>(__visitor),
           std::forward<_V0>(__v0));
      }
    else
      __builtin_unreachable();
  default:
    __builtin_unreachable();
  }


     }
 }
    }


  template<typename _Visitor, typename... _Variants>
    constexpr __detail::__variant::__visit_result_t<_Visitor, _Variants...>
    visit(_Visitor&& __visitor, _Variants&&... __variants)
    {
      namespace __variant = std::__detail::__variant;

      if ((__variant::__as(__variants).valueless_by_exception() || ...))
 __throw_bad_variant_access(2);

      using _Result_type
 = __detail::__variant::__visit_result_t<_Visitor, _Variants...>;

      using _Tag = __detail::__variant::__deduce_visit_result<_Result_type>;

      if constexpr (sizeof...(_Variants) == 1)
 {
   using _Vp = decltype(__variant::__as(std::declval<_Variants>()...));

   constexpr bool __visit_rettypes_match = __detail::__variant::
     __check_visitor_results<_Visitor, _Vp>(
       make_index_sequence<variant_size_v<remove_reference_t<_Vp>>>());
   if constexpr (!__visit_rettypes_match)
     {
       static_assert(__visit_rettypes_match,
     "std::visit requires the visitor to have the same "
     "return type for all alternatives of a variant");
       return;
     }
   else
     return std::__do_visit<_Tag>(
       std::forward<_Visitor>(__visitor),
       static_cast<_Vp>(__variants)...);
 }
      else
 return std::__do_visit<_Tag>(
   std::forward<_Visitor>(__visitor),
   __variant::__as(std::forward<_Variants>(__variants))...);
    }
# 1980 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/variant" 3
  template<typename... _Types>
    struct __variant_hash
    {

      using result_type [[__deprecated__]] = size_t;
      using argument_type [[__deprecated__]] = variant<_Types...>;


      size_t
      operator()(const variant<_Types...>& __t) const
      noexcept((is_nothrow_invocable_v<hash<decay_t<_Types>>, _Types> && ...))
      {
 size_t __ret;
 __detail::__variant::__raw_visit(
   [&__t, &__ret](auto&& __t_mem) mutable
   {
     using _Type = __remove_cvref_t<decltype(__t_mem)>;
     if constexpr (!is_same_v<_Type,
                __detail::__variant::__variant_cookie>)
       __ret = std::hash<size_t>{}(__t.index())
        + std::hash<_Type>{}(__t_mem);
     else
       __ret = std::hash<size_t>{}(__t.index());
   }, __t);
 return __ret;
      }
    };


  template<typename... _Types>
    struct hash<variant<_Types...>>
    : __conditional_t<(__is_hash_enabled_for<remove_const_t<_Types>> && ...),
        __variant_hash<_Types...>,
        __hash_not_enabled<variant<_Types...>>>

      , __detail::__variant::_Variant_hash_base<variant<_Types...>,
      index_sequence_for<_Types...>>

    { };

  template<typename... _Types>
    struct __is_fast_hash<hash<variant<_Types...>>>
    : bool_constant<(__is_fast_hash<_Types>::value && ...)>
    { };


}
# 6 "./cpp1z/variant1.C" 2


# 7 "./cpp1z/variant1.C"
struct A {};
struct B {};
struct C {};
struct D {};
struct E {};
struct F {};
struct G {};
struct H {};
struct I {};
struct J {};
struct K {};
struct L {};
struct M {};
struct N {};
struct O {};
struct P {};
struct Q {};
struct R {};
struct S {};
struct T {};
struct U {};
struct V {};
struct W {


    int i;
};
struct X {};
struct Y {};
struct Z {};

using Foo = std::variant<A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z>;

struct Bar {
    Foo f;
    static Bar dummy() {

        return {Z{}};

    }
};
