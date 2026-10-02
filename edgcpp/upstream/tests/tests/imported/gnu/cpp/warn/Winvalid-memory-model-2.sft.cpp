//type: fp
//options: --c++11
# 0 "./warn/Winvalid-memory-model-2.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./warn/Winvalid-memory-model-2.C"






# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/atomic" 1 3
# 50 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/atomic" 3
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
# 945 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
#pragma GCC diagnostic pop
# 52 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/version.h" 2 3
# 51 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/atomic" 2 3

# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h" 1 3
# 38 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h" 3
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
# 39 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_lockfree_defines.h" 1 3
# 40 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/move.h" 1 3
# 37 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/move.h" 3
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
# 38 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/move.h" 2 3


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
# 41 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h" 2 3
# 50 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/version.h" 1 3
# 51 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h" 2 3

namespace std __attribute__ ((__visibility__ ("default")))
{

# 82 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h" 3
  enum memory_order : int
    {
      memory_order_relaxed,
      memory_order_consume,
      memory_order_acquire,
      memory_order_release,
      memory_order_acq_rel,
      memory_order_seq_cst
    };



  enum __memory_order_modifier
    {
      __memory_order_mask = 0x0ffff,
      __memory_order_modifier_mask = 0xffff0000,
      __memory_order_hle_acquire = 0x10000,
      __memory_order_hle_release = 0x20000
    };


  constexpr memory_order
  operator|(memory_order __m, __memory_order_modifier __mod) noexcept
  {
    return memory_order(int(__m) | int(__mod));
  }

  constexpr memory_order
  operator&(memory_order __m, __memory_order_modifier __mod) noexcept
  {
    return memory_order(int(__m) & int(__mod));
  }




  constexpr memory_order
  __cmpexch_failure_order2(memory_order __m) noexcept
  {
    return __m == memory_order_acq_rel ? memory_order_acquire
      : __m == memory_order_release ? memory_order_relaxed : __m;
  }

  constexpr memory_order
  __cmpexch_failure_order(memory_order __m) noexcept
  {
    return memory_order(__cmpexch_failure_order2(__m & __memory_order_mask)
      | __memory_order_modifier(__m & __memory_order_modifier_mask));
  }

  constexpr bool
  __is_valid_cmpexch_failure_order(memory_order __m) noexcept
  {
    return (__m & __memory_order_mask) != memory_order_release
 && (__m & __memory_order_mask) != memory_order_acq_rel;
  }


  template<typename _IntTp>
    struct __atomic_base;



  inline __attribute__((__always_inline__)) void
  atomic_thread_fence(memory_order __m) noexcept
  { __atomic_thread_fence(int(__m)); }

  inline __attribute__((__always_inline__)) void
  atomic_signal_fence(memory_order __m) noexcept
  { __atomic_signal_fence(int(__m)); }


  template<typename _Tp>
    inline _Tp
    kill_dependency(_Tp __y) noexcept
    {
      _Tp __ret(__y);
      return __ret;
    }
# 172 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h" 3
  template<typename _Tp>
    struct atomic;

  template<typename _Tp>
    struct atomic<_Tp*>;



    typedef bool __atomic_flag_data_type;
# 197 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h" 3
  extern "C" {

  struct __atomic_flag_base
  {
    __atomic_flag_data_type _M_i ;
  };

  }






  struct atomic_flag : public __atomic_flag_base
  {
    atomic_flag() noexcept = default;
    ~atomic_flag() noexcept = default;
    atomic_flag(const atomic_flag&) = delete;
    atomic_flag& operator=(const atomic_flag&) = delete;
    atomic_flag& operator=(const atomic_flag&) volatile = delete;


    constexpr atomic_flag(bool __i) noexcept
      : __atomic_flag_base{ _S_init(__i) }
    { }

    inline __attribute__((__always_inline__)) bool
    test_and_set(memory_order __m = memory_order_seq_cst) noexcept
    {
      return __atomic_test_and_set (&_M_i, int(__m));
    }

    inline __attribute__((__always_inline__)) bool
    test_and_set(memory_order __m = memory_order_seq_cst) volatile noexcept
    {
      return __atomic_test_and_set (&_M_i, int(__m));
    }
# 281 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h" 3
    inline __attribute__((__always_inline__)) void
    clear(memory_order __m = memory_order_seq_cst) noexcept
    {
      memory_order __b __attribute__ ((__unused__))
 = __m & __memory_order_mask;
      do { if (__builtin_expect(!bool(__b != memory_order_consume), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 286, __PRETTY_FUNCTION__, "__b != memory_order_consume"); } while (false);
      do { if (__builtin_expect(!bool(__b != memory_order_acquire), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 287, __PRETTY_FUNCTION__, "__b != memory_order_acquire"); } while (false);
      do { if (__builtin_expect(!bool(__b != memory_order_acq_rel), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 288, __PRETTY_FUNCTION__, "__b != memory_order_acq_rel"); } while (false);

      __atomic_clear (&_M_i, int(__m));
    }

    inline __attribute__((__always_inline__)) void
    clear(memory_order __m = memory_order_seq_cst) volatile noexcept
    {
      memory_order __b __attribute__ ((__unused__))
 = __m & __memory_order_mask;
      do { if (__builtin_expect(!bool(__b != memory_order_consume), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 298, __PRETTY_FUNCTION__, "__b != memory_order_consume"); } while (false);
      do { if (__builtin_expect(!bool(__b != memory_order_acquire), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 299, __PRETTY_FUNCTION__, "__b != memory_order_acquire"); } while (false);
      do { if (__builtin_expect(!bool(__b != memory_order_acq_rel), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 300, __PRETTY_FUNCTION__, "__b != memory_order_acq_rel"); } while (false);

      __atomic_clear (&_M_i, int(__m));
    }

  private:
    static constexpr __atomic_flag_data_type
    _S_init(bool __i)
    { return __i ? 1 : 0; }
  };
# 337 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h" 3
  template<typename _ITp>
    struct __atomic_base
    {
      using value_type = _ITp;
      using difference_type = value_type;

    private:
      typedef _ITp __int_type;

      static constexpr int _S_alignment =
 sizeof(_ITp) > alignof(_ITp) ? sizeof(_ITp) : alignof(_ITp);

      alignas(_S_alignment) __int_type _M_i ;

    public:
      __atomic_base() noexcept = default;
      ~__atomic_base() noexcept = default;
      __atomic_base(const __atomic_base&) = delete;
      __atomic_base& operator=(const __atomic_base&) = delete;
      __atomic_base& operator=(const __atomic_base&) volatile = delete;


      constexpr __atomic_base(__int_type __i) noexcept : _M_i (__i) { }

      operator __int_type() const noexcept
      { return load(); }

      operator __int_type() const volatile noexcept
      { return load(); }

      __int_type
      operator=(__int_type __i) noexcept
      {
 store(__i);
 return __i;
      }

      __int_type
      operator=(__int_type __i) volatile noexcept
      {
 store(__i);
 return __i;
      }

      __int_type
      operator++(int) noexcept
      { return fetch_add(1); }

      __int_type
      operator++(int) volatile noexcept
      { return fetch_add(1); }

      __int_type
      operator--(int) noexcept
      { return fetch_sub(1); }

      __int_type
      operator--(int) volatile noexcept
      { return fetch_sub(1); }

      __int_type
      operator++() noexcept
      { return __atomic_add_fetch(&_M_i, 1, int(memory_order_seq_cst)); }

      __int_type
      operator++() volatile noexcept
      { return __atomic_add_fetch(&_M_i, 1, int(memory_order_seq_cst)); }

      __int_type
      operator--() noexcept
      { return __atomic_sub_fetch(&_M_i, 1, int(memory_order_seq_cst)); }

      __int_type
      operator--() volatile noexcept
      { return __atomic_sub_fetch(&_M_i, 1, int(memory_order_seq_cst)); }

      __int_type
      operator+=(__int_type __i) noexcept
      { return __atomic_add_fetch(&_M_i, __i, int(memory_order_seq_cst)); }

      __int_type
      operator+=(__int_type __i) volatile noexcept
      { return __atomic_add_fetch(&_M_i, __i, int(memory_order_seq_cst)); }

      __int_type
      operator-=(__int_type __i) noexcept
      { return __atomic_sub_fetch(&_M_i, __i, int(memory_order_seq_cst)); }

      __int_type
      operator-=(__int_type __i) volatile noexcept
      { return __atomic_sub_fetch(&_M_i, __i, int(memory_order_seq_cst)); }

      __int_type
      operator&=(__int_type __i) noexcept
      { return __atomic_and_fetch(&_M_i, __i, int(memory_order_seq_cst)); }

      __int_type
      operator&=(__int_type __i) volatile noexcept
      { return __atomic_and_fetch(&_M_i, __i, int(memory_order_seq_cst)); }

      __int_type
      operator|=(__int_type __i) noexcept
      { return __atomic_or_fetch(&_M_i, __i, int(memory_order_seq_cst)); }

      __int_type
      operator|=(__int_type __i) volatile noexcept
      { return __atomic_or_fetch(&_M_i, __i, int(memory_order_seq_cst)); }

      __int_type
      operator^=(__int_type __i) noexcept
      { return __atomic_xor_fetch(&_M_i, __i, int(memory_order_seq_cst)); }

      __int_type
      operator^=(__int_type __i) volatile noexcept
      { return __atomic_xor_fetch(&_M_i, __i, int(memory_order_seq_cst)); }

      bool
      is_lock_free() const noexcept
      {

 return __atomic_is_lock_free(sizeof(_M_i),
     reinterpret_cast<void *>(-_S_alignment));
      }

      bool
      is_lock_free() const volatile noexcept
      {

 return __atomic_is_lock_free(sizeof(_M_i),
     reinterpret_cast<void *>(-_S_alignment));
      }

      inline __attribute__((__always_inline__)) void
      store(__int_type __i, memory_order __m = memory_order_seq_cst) noexcept
      {
 memory_order __b __attribute__ ((__unused__))
   = __m & __memory_order_mask;
 do { if (__builtin_expect(!bool(__b != memory_order_acquire), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 474, __PRETTY_FUNCTION__, "__b != memory_order_acquire"); } while (false);
 do { if (__builtin_expect(!bool(__b != memory_order_acq_rel), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 475, __PRETTY_FUNCTION__, "__b != memory_order_acq_rel"); } while (false);
 do { if (__builtin_expect(!bool(__b != memory_order_consume), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 476, __PRETTY_FUNCTION__, "__b != memory_order_consume"); } while (false);

 __atomic_store_n(&_M_i, __i, int(__m));
      }

      inline __attribute__((__always_inline__)) void
      store(__int_type __i,
     memory_order __m = memory_order_seq_cst) volatile noexcept
      {
 memory_order __b __attribute__ ((__unused__))
   = __m & __memory_order_mask;
 do { if (__builtin_expect(!bool(__b != memory_order_acquire), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 487, __PRETTY_FUNCTION__, "__b != memory_order_acquire"); } while (false);
 do { if (__builtin_expect(!bool(__b != memory_order_acq_rel), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 488, __PRETTY_FUNCTION__, "__b != memory_order_acq_rel"); } while (false);
 do { if (__builtin_expect(!bool(__b != memory_order_consume), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 489, __PRETTY_FUNCTION__, "__b != memory_order_consume"); } while (false);

 __atomic_store_n(&_M_i, __i, int(__m));
      }

      inline __attribute__((__always_inline__)) __int_type
      load(memory_order __m = memory_order_seq_cst) const noexcept
      {
 memory_order __b __attribute__ ((__unused__))
   = __m & __memory_order_mask;
 do { if (__builtin_expect(!bool(__b != memory_order_release), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 499, __PRETTY_FUNCTION__, "__b != memory_order_release"); } while (false);
 do { if (__builtin_expect(!bool(__b != memory_order_acq_rel), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 500, __PRETTY_FUNCTION__, "__b != memory_order_acq_rel"); } while (false);

 return __atomic_load_n(&_M_i, int(__m));
      }

      inline __attribute__((__always_inline__)) __int_type
      load(memory_order __m = memory_order_seq_cst) const volatile noexcept
      {
 memory_order __b __attribute__ ((__unused__))
   = __m & __memory_order_mask;
 do { if (__builtin_expect(!bool(__b != memory_order_release), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 510, __PRETTY_FUNCTION__, "__b != memory_order_release"); } while (false);
 do { if (__builtin_expect(!bool(__b != memory_order_acq_rel), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 511, __PRETTY_FUNCTION__, "__b != memory_order_acq_rel"); } while (false);

 return __atomic_load_n(&_M_i, int(__m));
      }

      inline __attribute__((__always_inline__)) __int_type
      exchange(__int_type __i,
        memory_order __m = memory_order_seq_cst) noexcept
      {
 return __atomic_exchange_n(&_M_i, __i, int(__m));
      }


      inline __attribute__((__always_inline__)) __int_type
      exchange(__int_type __i,
        memory_order __m = memory_order_seq_cst) volatile noexcept
      {
 return __atomic_exchange_n(&_M_i, __i, int(__m));
      }

      inline __attribute__((__always_inline__)) bool
      compare_exchange_weak(__int_type& __i1, __int_type __i2,
       memory_order __m1, memory_order __m2) noexcept
      {
 do { if (__builtin_expect(!bool(__is_valid_cmpexch_failure_order(__m2)), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 535, __PRETTY_FUNCTION__, "__is_valid_cmpexch_failure_order(__m2)"); } while (false);

 return __atomic_compare_exchange_n(&_M_i, &__i1, __i2, 1,
        int(__m1), int(__m2));
      }

      inline __attribute__((__always_inline__)) bool
      compare_exchange_weak(__int_type& __i1, __int_type __i2,
       memory_order __m1,
       memory_order __m2) volatile noexcept
      {
 do { if (__builtin_expect(!bool(__is_valid_cmpexch_failure_order(__m2)), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 546, __PRETTY_FUNCTION__, "__is_valid_cmpexch_failure_order(__m2)"); } while (false);

 return __atomic_compare_exchange_n(&_M_i, &__i1, __i2, 1,
        int(__m1), int(__m2));
      }

      inline __attribute__((__always_inline__)) bool
      compare_exchange_weak(__int_type& __i1, __int_type __i2,
       memory_order __m = memory_order_seq_cst) noexcept
      {
 return compare_exchange_weak(__i1, __i2, __m,
         __cmpexch_failure_order(__m));
      }

      inline __attribute__((__always_inline__)) bool
      compare_exchange_weak(__int_type& __i1, __int_type __i2,
     memory_order __m = memory_order_seq_cst) volatile noexcept
      {
 return compare_exchange_weak(__i1, __i2, __m,
         __cmpexch_failure_order(__m));
      }

      inline __attribute__((__always_inline__)) bool
      compare_exchange_strong(__int_type& __i1, __int_type __i2,
         memory_order __m1, memory_order __m2) noexcept
      {
 do { if (__builtin_expect(!bool(__is_valid_cmpexch_failure_order(__m2)), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 572, __PRETTY_FUNCTION__, "__is_valid_cmpexch_failure_order(__m2)"); } while (false);

 return __atomic_compare_exchange_n(&_M_i, &__i1, __i2, 0,
        int(__m1), int(__m2));
      }

      inline __attribute__((__always_inline__)) bool
      compare_exchange_strong(__int_type& __i1, __int_type __i2,
         memory_order __m1,
         memory_order __m2) volatile noexcept
      {
 do { if (__builtin_expect(!bool(__is_valid_cmpexch_failure_order(__m2)), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 583, __PRETTY_FUNCTION__, "__is_valid_cmpexch_failure_order(__m2)"); } while (false);

 return __atomic_compare_exchange_n(&_M_i, &__i1, __i2, 0,
        int(__m1), int(__m2));
      }

      inline __attribute__((__always_inline__)) bool
      compare_exchange_strong(__int_type& __i1, __int_type __i2,
         memory_order __m = memory_order_seq_cst) noexcept
      {
 return compare_exchange_strong(__i1, __i2, __m,
           __cmpexch_failure_order(__m));
      }

      inline __attribute__((__always_inline__)) bool
      compare_exchange_strong(__int_type& __i1, __int_type __i2,
   memory_order __m = memory_order_seq_cst) volatile noexcept
      {
 return compare_exchange_strong(__i1, __i2, __m,
           __cmpexch_failure_order(__m));
      }
# 629 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h" 3
      inline __attribute__((__always_inline__)) __int_type
      fetch_add(__int_type __i,
  memory_order __m = memory_order_seq_cst) noexcept
      { return __atomic_fetch_add(&_M_i, __i, int(__m)); }

      inline __attribute__((__always_inline__)) __int_type
      fetch_add(__int_type __i,
  memory_order __m = memory_order_seq_cst) volatile noexcept
      { return __atomic_fetch_add(&_M_i, __i, int(__m)); }

      inline __attribute__((__always_inline__)) __int_type
      fetch_sub(__int_type __i,
  memory_order __m = memory_order_seq_cst) noexcept
      { return __atomic_fetch_sub(&_M_i, __i, int(__m)); }

      inline __attribute__((__always_inline__)) __int_type
      fetch_sub(__int_type __i,
  memory_order __m = memory_order_seq_cst) volatile noexcept
      { return __atomic_fetch_sub(&_M_i, __i, int(__m)); }

      inline __attribute__((__always_inline__)) __int_type
      fetch_and(__int_type __i,
  memory_order __m = memory_order_seq_cst) noexcept
      { return __atomic_fetch_and(&_M_i, __i, int(__m)); }

      inline __attribute__((__always_inline__)) __int_type
      fetch_and(__int_type __i,
  memory_order __m = memory_order_seq_cst) volatile noexcept
      { return __atomic_fetch_and(&_M_i, __i, int(__m)); }

      inline __attribute__((__always_inline__)) __int_type
      fetch_or(__int_type __i,
        memory_order __m = memory_order_seq_cst) noexcept
      { return __atomic_fetch_or(&_M_i, __i, int(__m)); }

      inline __attribute__((__always_inline__)) __int_type
      fetch_or(__int_type __i,
        memory_order __m = memory_order_seq_cst) volatile noexcept
      { return __atomic_fetch_or(&_M_i, __i, int(__m)); }

      inline __attribute__((__always_inline__)) __int_type
      fetch_xor(__int_type __i,
  memory_order __m = memory_order_seq_cst) noexcept
      { return __atomic_fetch_xor(&_M_i, __i, int(__m)); }

      inline __attribute__((__always_inline__)) __int_type
      fetch_xor(__int_type __i,
  memory_order __m = memory_order_seq_cst) volatile noexcept
      { return __atomic_fetch_xor(&_M_i, __i, int(__m)); }
    };



  template<typename _PTp>
    struct __atomic_base<_PTp*>
    {
    private:
      typedef _PTp* __pointer_type;

      __pointer_type _M_p ;

      static constexpr ptrdiff_t
      _S_type_size(ptrdiff_t __d)
      { return __d * sizeof(_PTp); }

    public:
      __atomic_base() noexcept = default;
      ~__atomic_base() noexcept = default;
      __atomic_base(const __atomic_base&) = delete;
      __atomic_base& operator=(const __atomic_base&) = delete;
      __atomic_base& operator=(const __atomic_base&) volatile = delete;


      constexpr __atomic_base(__pointer_type __p) noexcept : _M_p (__p) { }

      operator __pointer_type() const noexcept
      { return load(); }

      operator __pointer_type() const volatile noexcept
      { return load(); }

      __pointer_type
      operator=(__pointer_type __p) noexcept
      {
 store(__p);
 return __p;
      }

      __pointer_type
      operator=(__pointer_type __p) volatile noexcept
      {
 store(__p);
 return __p;
      }

      __pointer_type
      operator++(int) noexcept
      { return fetch_add(1); }

      __pointer_type
      operator++(int) volatile noexcept
      { return fetch_add(1); }

      __pointer_type
      operator--(int) noexcept
      { return fetch_sub(1); }

      __pointer_type
      operator--(int) volatile noexcept
      { return fetch_sub(1); }

      __pointer_type
      operator++() noexcept
      { return __atomic_add_fetch(&_M_p, _S_type_size(1),
      int(memory_order_seq_cst)); }

      __pointer_type
      operator++() volatile noexcept
      { return __atomic_add_fetch(&_M_p, _S_type_size(1),
      int(memory_order_seq_cst)); }

      __pointer_type
      operator--() noexcept
      { return __atomic_sub_fetch(&_M_p, _S_type_size(1),
      int(memory_order_seq_cst)); }

      __pointer_type
      operator--() volatile noexcept
      { return __atomic_sub_fetch(&_M_p, _S_type_size(1),
      int(memory_order_seq_cst)); }

      __pointer_type
      operator+=(ptrdiff_t __d) noexcept
      { return __atomic_add_fetch(&_M_p, _S_type_size(__d),
      int(memory_order_seq_cst)); }

      __pointer_type
      operator+=(ptrdiff_t __d) volatile noexcept
      { return __atomic_add_fetch(&_M_p, _S_type_size(__d),
      int(memory_order_seq_cst)); }

      __pointer_type
      operator-=(ptrdiff_t __d) noexcept
      { return __atomic_sub_fetch(&_M_p, _S_type_size(__d),
      int(memory_order_seq_cst)); }

      __pointer_type
      operator-=(ptrdiff_t __d) volatile noexcept
      { return __atomic_sub_fetch(&_M_p, _S_type_size(__d),
      int(memory_order_seq_cst)); }

      bool
      is_lock_free() const noexcept
      {

 return __atomic_is_lock_free(sizeof(_M_p),
     reinterpret_cast<void *>(-__alignof(_M_p)));
      }

      bool
      is_lock_free() const volatile noexcept
      {

 return __atomic_is_lock_free(sizeof(_M_p),
     reinterpret_cast<void *>(-__alignof(_M_p)));
      }

      inline __attribute__((__always_inline__)) void
      store(__pointer_type __p,
     memory_order __m = memory_order_seq_cst) noexcept
      {
 memory_order __b __attribute__ ((__unused__))
   = __m & __memory_order_mask;

 do { if (__builtin_expect(!bool(__b != memory_order_acquire), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 803, __PRETTY_FUNCTION__, "__b != memory_order_acquire"); } while (false);
 do { if (__builtin_expect(!bool(__b != memory_order_acq_rel), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 804, __PRETTY_FUNCTION__, "__b != memory_order_acq_rel"); } while (false);
 do { if (__builtin_expect(!bool(__b != memory_order_consume), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 805, __PRETTY_FUNCTION__, "__b != memory_order_consume"); } while (false);

 __atomic_store_n(&_M_p, __p, int(__m));
      }

      inline __attribute__((__always_inline__)) void
      store(__pointer_type __p,
     memory_order __m = memory_order_seq_cst) volatile noexcept
      {
 memory_order __b __attribute__ ((__unused__))
   = __m & __memory_order_mask;
 do { if (__builtin_expect(!bool(__b != memory_order_acquire), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 816, __PRETTY_FUNCTION__, "__b != memory_order_acquire"); } while (false);
 do { if (__builtin_expect(!bool(__b != memory_order_acq_rel), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 817, __PRETTY_FUNCTION__, "__b != memory_order_acq_rel"); } while (false);
 do { if (__builtin_expect(!bool(__b != memory_order_consume), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 818, __PRETTY_FUNCTION__, "__b != memory_order_consume"); } while (false);

 __atomic_store_n(&_M_p, __p, int(__m));
      }

      inline __attribute__((__always_inline__)) __pointer_type
      load(memory_order __m = memory_order_seq_cst) const noexcept
      {
 memory_order __b __attribute__ ((__unused__))
   = __m & __memory_order_mask;
 do { if (__builtin_expect(!bool(__b != memory_order_release), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 828, __PRETTY_FUNCTION__, "__b != memory_order_release"); } while (false);
 do { if (__builtin_expect(!bool(__b != memory_order_acq_rel), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 829, __PRETTY_FUNCTION__, "__b != memory_order_acq_rel"); } while (false);

 return __atomic_load_n(&_M_p, int(__m));
      }

      inline __attribute__((__always_inline__)) __pointer_type
      load(memory_order __m = memory_order_seq_cst) const volatile noexcept
      {
 memory_order __b __attribute__ ((__unused__))
   = __m & __memory_order_mask;
 do { if (__builtin_expect(!bool(__b != memory_order_release), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 839, __PRETTY_FUNCTION__, "__b != memory_order_release"); } while (false);
 do { if (__builtin_expect(!bool(__b != memory_order_acq_rel), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 840, __PRETTY_FUNCTION__, "__b != memory_order_acq_rel"); } while (false);

 return __atomic_load_n(&_M_p, int(__m));
      }

      inline __attribute__((__always_inline__)) __pointer_type
      exchange(__pointer_type __p,
        memory_order __m = memory_order_seq_cst) noexcept
      {
 return __atomic_exchange_n(&_M_p, __p, int(__m));
      }


      inline __attribute__((__always_inline__)) __pointer_type
      exchange(__pointer_type __p,
        memory_order __m = memory_order_seq_cst) volatile noexcept
      {
 return __atomic_exchange_n(&_M_p, __p, int(__m));
      }

      inline __attribute__((__always_inline__)) bool
      compare_exchange_weak(__pointer_type& __p1, __pointer_type __p2,
       memory_order __m1,
       memory_order __m2) noexcept
      {
 do { if (__builtin_expect(!bool(__is_valid_cmpexch_failure_order(__m2)), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 865, __PRETTY_FUNCTION__, "__is_valid_cmpexch_failure_order(__m2)"); } while (false);

 return __atomic_compare_exchange_n(&_M_p, &__p1, __p2, 1,
        int(__m1), int(__m2));
      }

      inline __attribute__((__always_inline__)) bool
      compare_exchange_weak(__pointer_type& __p1, __pointer_type __p2,
       memory_order __m1,
       memory_order __m2) volatile noexcept
      {
 do { if (__builtin_expect(!bool(__is_valid_cmpexch_failure_order(__m2)), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 876, __PRETTY_FUNCTION__, "__is_valid_cmpexch_failure_order(__m2)"); } while (false);

 return __atomic_compare_exchange_n(&_M_p, &__p1, __p2, 1,
        int(__m1), int(__m2));
      }

      inline __attribute__((__always_inline__)) bool
      compare_exchange_strong(__pointer_type& __p1, __pointer_type __p2,
         memory_order __m1,
         memory_order __m2) noexcept
      {
 do { if (__builtin_expect(!bool(__is_valid_cmpexch_failure_order(__m2)), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 887, __PRETTY_FUNCTION__, "__is_valid_cmpexch_failure_order(__m2)"); } while (false);

 return __atomic_compare_exchange_n(&_M_p, &__p1, __p2, 0,
        int(__m1), int(__m2));
      }

      inline __attribute__((__always_inline__)) bool
      compare_exchange_strong(__pointer_type& __p1, __pointer_type __p2,
         memory_order __m1,
         memory_order __m2) volatile noexcept
      {
 do { if (__builtin_expect(!bool(__is_valid_cmpexch_failure_order(__m2)), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 898, __PRETTY_FUNCTION__, "__is_valid_cmpexch_failure_order(__m2)"); } while (false);

 return __atomic_compare_exchange_n(&_M_p, &__p1, __p2, 0,
        int(__m1), int(__m2));
      }
# 929 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h" 3
      inline __attribute__((__always_inline__)) __pointer_type
      fetch_add(ptrdiff_t __d,
  memory_order __m = memory_order_seq_cst) noexcept
      { return __atomic_fetch_add(&_M_p, _S_type_size(__d), int(__m)); }

      inline __attribute__((__always_inline__)) __pointer_type
      fetch_add(ptrdiff_t __d,
  memory_order __m = memory_order_seq_cst) volatile noexcept
      { return __atomic_fetch_add(&_M_p, _S_type_size(__d), int(__m)); }

      inline __attribute__((__always_inline__)) __pointer_type
      fetch_sub(ptrdiff_t __d,
  memory_order __m = memory_order_seq_cst) noexcept
      { return __atomic_fetch_sub(&_M_p, _S_type_size(__d), int(__m)); }

      inline __attribute__((__always_inline__)) __pointer_type
      fetch_sub(ptrdiff_t __d,
  memory_order __m = memory_order_seq_cst) volatile noexcept
      { return __atomic_fetch_sub(&_M_p, _S_type_size(__d), int(__m)); }
    };

  namespace __atomic_impl
  {


    template<typename _Tp>
      constexpr bool
      __maybe_has_padding()
      {



 return !__has_unique_object_representations(_Tp)
   && !is_same<_Tp, float>::value && !is_same<_Tp, double>::value;



      }

    template<typename _Tp>
      inline __attribute__((__always_inline__)) _Tp*
      __clear_padding(_Tp& __val) noexcept
      {
 auto* __ptr = std::__addressof(__val);

 if (__atomic_impl::__maybe_has_padding<_Tp>())
   __builtin_clear_padding(__ptr);

 return __ptr;
      }


    template<typename _Tp>
      using _Val = typename remove_volatile<_Tp>::type;

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++17-extensions"

    template<bool _AtomicRef = false, typename _Tp>
      inline __attribute__((__always_inline__)) bool
      __compare_exchange(_Tp& __val, _Val<_Tp>& __e, _Val<_Tp>& __i,
    bool __is_weak,
    memory_order __s, memory_order __f) noexcept
      {
 do { if (__builtin_expect(!bool(__is_valid_cmpexch_failure_order(__f)), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h", 993, __PRETTY_FUNCTION__, "__is_valid_cmpexch_failure_order(__f)"); } while (false);

 using _Vp = _Val<_Tp>;
 _Tp* const __pval = std::__addressof(__val);

 if constexpr (!__atomic_impl::__maybe_has_padding<_Vp>())
   {
     return __atomic_compare_exchange(__pval, std::__addressof(__e),
          std::__addressof(__i), __is_weak,
          int(__s), int(__f));
   }
 else if constexpr (!_AtomicRef)
   {

     _Vp* const __pi = __atomic_impl::__clear_padding(__i);

     _Vp __exp = __e;

     _Vp* const __pexp = __atomic_impl::__clear_padding(__exp);



     if (__atomic_compare_exchange(__pval, __pexp, __pi,
       __is_weak, int(__s), int(__f)))
       return true;

     __builtin_memcpy(std::__addressof(__e), __pexp, sizeof(_Vp));
     return false;
   }
 else
   {

     _Vp* const __pi = __atomic_impl::__clear_padding(__i);


     _Vp __exp = __e;


     _Vp* const __pexp = __atomic_impl::__clear_padding(__exp);
# 1043 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h" 3
     while (true)
       {

  _Vp __orig = __exp;

  if (__atomic_compare_exchange(__pval, __pexp, __pi,
           __is_weak, int(__s), int(__f)))
    return true;


  _Vp __curr = __exp;


  if (__builtin_memcmp(__atomic_impl::__clear_padding(__orig),
         __atomic_impl::__clear_padding(__curr),
         sizeof(_Vp)))
    {

      __builtin_memcpy(std::__addressof(__e), __pexp,
         sizeof(_Vp));
      return false;
    }
       }
   }
      }
#pragma GCC diagnostic pop
  }
# 2070 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/atomic_base.h" 3

}
# 53 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/atomic" 2 3
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
# 54 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/atomic" 2 3

namespace std __attribute__ ((__visibility__ ("default")))
{







  template<typename _Tp>
    struct atomic;



  template<>
  struct atomic<bool>
  {
    using value_type = bool;

  private:
    __atomic_base<bool> _M_base;

  public:
    atomic() noexcept = default;
    ~atomic() noexcept = default;
    atomic(const atomic&) = delete;
    atomic& operator=(const atomic&) = delete;
    atomic& operator=(const atomic&) volatile = delete;

    constexpr atomic(bool __i) noexcept : _M_base(__i) { }

    bool
    operator=(bool __i) noexcept
    { return _M_base.operator=(__i); }

    bool
    operator=(bool __i) volatile noexcept
    { return _M_base.operator=(__i); }

    operator bool() const noexcept
    { return _M_base.load(); }

    operator bool() const volatile noexcept
    { return _M_base.load(); }

    bool
    is_lock_free() const noexcept { return _M_base.is_lock_free(); }

    bool
    is_lock_free() const volatile noexcept { return _M_base.is_lock_free(); }





    void
    store(bool __i, memory_order __m = memory_order_seq_cst) noexcept
    { _M_base.store(__i, __m); }

    void
    store(bool __i, memory_order __m = memory_order_seq_cst) volatile noexcept
    { _M_base.store(__i, __m); }

    bool
    load(memory_order __m = memory_order_seq_cst) const noexcept
    { return _M_base.load(__m); }

    bool
    load(memory_order __m = memory_order_seq_cst) const volatile noexcept
    { return _M_base.load(__m); }

    bool
    exchange(bool __i, memory_order __m = memory_order_seq_cst) noexcept
    { return _M_base.exchange(__i, __m); }

    bool
    exchange(bool __i,
      memory_order __m = memory_order_seq_cst) volatile noexcept
    { return _M_base.exchange(__i, __m); }

    bool
    compare_exchange_weak(bool& __i1, bool __i2, memory_order __m1,
     memory_order __m2) noexcept
    { return _M_base.compare_exchange_weak(__i1, __i2, __m1, __m2); }

    bool
    compare_exchange_weak(bool& __i1, bool __i2, memory_order __m1,
     memory_order __m2) volatile noexcept
    { return _M_base.compare_exchange_weak(__i1, __i2, __m1, __m2); }

    bool
    compare_exchange_weak(bool& __i1, bool __i2,
     memory_order __m = memory_order_seq_cst) noexcept
    { return _M_base.compare_exchange_weak(__i1, __i2, __m); }

    bool
    compare_exchange_weak(bool& __i1, bool __i2,
       memory_order __m = memory_order_seq_cst) volatile noexcept
    { return _M_base.compare_exchange_weak(__i1, __i2, __m); }

    bool
    compare_exchange_strong(bool& __i1, bool __i2, memory_order __m1,
       memory_order __m2) noexcept
    { return _M_base.compare_exchange_strong(__i1, __i2, __m1, __m2); }

    bool
    compare_exchange_strong(bool& __i1, bool __i2, memory_order __m1,
       memory_order __m2) volatile noexcept
    { return _M_base.compare_exchange_strong(__i1, __i2, __m1, __m2); }

    bool
    compare_exchange_strong(bool& __i1, bool __i2,
       memory_order __m = memory_order_seq_cst) noexcept
    { return _M_base.compare_exchange_strong(__i1, __i2, __m); }

    bool
    compare_exchange_strong(bool& __i1, bool __i2,
      memory_order __m = memory_order_seq_cst) volatile noexcept
    { return _M_base.compare_exchange_strong(__i1, __i2, __m); }
# 190 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/atomic" 3
  };
# 205 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/atomic" 3
  template<typename _Tp>
    struct atomic
    {
      using value_type = _Tp;

    private:

      static constexpr int _S_min_alignment
 = (sizeof(_Tp) & (sizeof(_Tp) - 1)) || sizeof(_Tp) > 16
 ? 0 : sizeof(_Tp);

      static constexpr int _S_alignment
        = _S_min_alignment > alignof(_Tp) ? _S_min_alignment : alignof(_Tp);

      alignas(_S_alignment) _Tp _M_i ;

      static_assert(__is_trivially_copyable(_Tp),
      "std::atomic requires a trivially copyable type");

      static_assert(sizeof(_Tp) > 0,
      "Incomplete or zero-sized types are not supported");
# 234 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/atomic" 3
    public:
      atomic() = default;
      ~atomic() noexcept = default;
      atomic(const atomic&) = delete;
      atomic& operator=(const atomic&) = delete;
      atomic& operator=(const atomic&) volatile = delete;

      constexpr atomic(_Tp __i) noexcept : _M_i(__i)
      {




      }

      operator _Tp() const noexcept
      { return load(); }

      operator _Tp() const volatile noexcept
      { return load(); }

      _Tp
      operator=(_Tp __i) noexcept
      { store(__i); return __i; }

      _Tp
      operator=(_Tp __i) volatile noexcept
      { store(__i); return __i; }

      bool
      is_lock_free() const noexcept
      {

 return __atomic_is_lock_free(sizeof(_M_i),
     reinterpret_cast<void *>(-_S_alignment));
      }

      bool
      is_lock_free() const volatile noexcept
      {

 return __atomic_is_lock_free(sizeof(_M_i),
     reinterpret_cast<void *>(-_S_alignment));
      }






      void
      store(_Tp __i, memory_order __m = memory_order_seq_cst) noexcept
      {
 __atomic_store(std::__addressof(_M_i),
         __atomic_impl::__clear_padding(__i),
         int(__m));
      }

      void
      store(_Tp __i, memory_order __m = memory_order_seq_cst) volatile noexcept
      {
 __atomic_store(std::__addressof(_M_i),
         __atomic_impl::__clear_padding(__i),
         int(__m));
      }

      _Tp
      load(memory_order __m = memory_order_seq_cst) const noexcept
      {
 alignas(_Tp) unsigned char __buf[sizeof(_Tp)];
 _Tp* __ptr = reinterpret_cast<_Tp*>(__buf);
 __atomic_load(std::__addressof(_M_i), __ptr, int(__m));
 return *__ptr;
      }

      _Tp
      load(memory_order __m = memory_order_seq_cst) const volatile noexcept
      {
        alignas(_Tp) unsigned char __buf[sizeof(_Tp)];
 _Tp* __ptr = reinterpret_cast<_Tp*>(__buf);
 __atomic_load(std::__addressof(_M_i), __ptr, int(__m));
 return *__ptr;
      }

      _Tp
      exchange(_Tp __i, memory_order __m = memory_order_seq_cst) noexcept
      {
        alignas(_Tp) unsigned char __buf[sizeof(_Tp)];
 _Tp* __ptr = reinterpret_cast<_Tp*>(__buf);
 __atomic_exchange(std::__addressof(_M_i),
     __atomic_impl::__clear_padding(__i),
     __ptr, int(__m));
 return *__ptr;
      }

      _Tp
      exchange(_Tp __i,
        memory_order __m = memory_order_seq_cst) volatile noexcept
      {
        alignas(_Tp) unsigned char __buf[sizeof(_Tp)];
 _Tp* __ptr = reinterpret_cast<_Tp*>(__buf);
 __atomic_exchange(std::__addressof(_M_i),
     __atomic_impl::__clear_padding(__i),
     __ptr, int(__m));
 return *__ptr;
      }

      bool
      compare_exchange_weak(_Tp& __e, _Tp __i, memory_order __s,
       memory_order __f) noexcept
      {
 return __atomic_impl::__compare_exchange(_M_i, __e, __i, true,
       __s, __f);
      }

      bool
      compare_exchange_weak(_Tp& __e, _Tp __i, memory_order __s,
       memory_order __f) volatile noexcept
      {
 return __atomic_impl::__compare_exchange(_M_i, __e, __i, true,
       __s, __f);
      }

      bool
      compare_exchange_weak(_Tp& __e, _Tp __i,
       memory_order __m = memory_order_seq_cst) noexcept
      { return compare_exchange_weak(__e, __i, __m,
                                     __cmpexch_failure_order(__m)); }

      bool
      compare_exchange_weak(_Tp& __e, _Tp __i,
       memory_order __m = memory_order_seq_cst) volatile noexcept
      { return compare_exchange_weak(__e, __i, __m,
                                     __cmpexch_failure_order(__m)); }

      bool
      compare_exchange_strong(_Tp& __e, _Tp __i, memory_order __s,
         memory_order __f) noexcept
      {
 return __atomic_impl::__compare_exchange(_M_i, __e, __i, false,
       __s, __f);
      }

      bool
      compare_exchange_strong(_Tp& __e, _Tp __i, memory_order __s,
         memory_order __f) volatile noexcept
      {
 return __atomic_impl::__compare_exchange(_M_i, __e, __i, false,
       __s, __f);
      }

      bool
      compare_exchange_strong(_Tp& __e, _Tp __i,
          memory_order __m = memory_order_seq_cst) noexcept
      { return compare_exchange_strong(__e, __i, __m,
                                       __cmpexch_failure_order(__m)); }

      bool
      compare_exchange_strong(_Tp& __e, _Tp __i,
       memory_order __m = memory_order_seq_cst) volatile noexcept
      { return compare_exchange_strong(__e, __i, __m,
                                       __cmpexch_failure_order(__m)); }
# 416 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/atomic" 3
    };



  template<typename _Tp>
    struct atomic<_Tp*>
    {
      using value_type = _Tp*;
      using difference_type = ptrdiff_t;

      typedef _Tp* __pointer_type;
      typedef __atomic_base<_Tp*> __base_type;
      __base_type _M_b;

      atomic() noexcept = default;
      ~atomic() noexcept = default;
      atomic(const atomic&) = delete;
      atomic& operator=(const atomic&) = delete;
      atomic& operator=(const atomic&) volatile = delete;

      constexpr atomic(__pointer_type __p) noexcept : _M_b(__p) { }

      operator __pointer_type() const noexcept
      { return __pointer_type(_M_b); }

      operator __pointer_type() const volatile noexcept
      { return __pointer_type(_M_b); }

      __pointer_type
      operator=(__pointer_type __p) noexcept
      { return _M_b.operator=(__p); }

      __pointer_type
      operator=(__pointer_type __p) volatile noexcept
      { return _M_b.operator=(__p); }

      __pointer_type
      operator++(int) noexcept
      {



 return _M_b++;
      }

      __pointer_type
      operator++(int) volatile noexcept
      {



 return _M_b++;
      }

      __pointer_type
      operator--(int) noexcept
      {



 return _M_b--;
      }

      __pointer_type
      operator--(int) volatile noexcept
      {



 return _M_b--;
      }

      __pointer_type
      operator++() noexcept
      {



 return ++_M_b;
      }

      __pointer_type
      operator++() volatile noexcept
      {



 return ++_M_b;
      }

      __pointer_type
      operator--() noexcept
      {



 return --_M_b;
      }

      __pointer_type
      operator--() volatile noexcept
      {



 return --_M_b;
      }

      __pointer_type
      operator+=(ptrdiff_t __d) noexcept
      {



 return _M_b.operator+=(__d);
      }

      __pointer_type
      operator+=(ptrdiff_t __d) volatile noexcept
      {



 return _M_b.operator+=(__d);
      }

      __pointer_type
      operator-=(ptrdiff_t __d) noexcept
      {



 return _M_b.operator-=(__d);
      }

      __pointer_type
      operator-=(ptrdiff_t __d) volatile noexcept
      {



 return _M_b.operator-=(__d);
      }

      bool
      is_lock_free() const noexcept
      { return _M_b.is_lock_free(); }

      bool
      is_lock_free() const volatile noexcept
      { return _M_b.is_lock_free(); }






      void
      store(__pointer_type __p,
     memory_order __m = memory_order_seq_cst) noexcept
      { return _M_b.store(__p, __m); }

      void
      store(__pointer_type __p,
     memory_order __m = memory_order_seq_cst) volatile noexcept
      { return _M_b.store(__p, __m); }

      __pointer_type
      load(memory_order __m = memory_order_seq_cst) const noexcept
      { return _M_b.load(__m); }

      __pointer_type
      load(memory_order __m = memory_order_seq_cst) const volatile noexcept
      { return _M_b.load(__m); }

      __pointer_type
      exchange(__pointer_type __p,
        memory_order __m = memory_order_seq_cst) noexcept
      { return _M_b.exchange(__p, __m); }

      __pointer_type
      exchange(__pointer_type __p,
        memory_order __m = memory_order_seq_cst) volatile noexcept
      { return _M_b.exchange(__p, __m); }

      bool
      compare_exchange_weak(__pointer_type& __p1, __pointer_type __p2,
       memory_order __m1, memory_order __m2) noexcept
      { return _M_b.compare_exchange_weak(__p1, __p2, __m1, __m2); }

      bool
      compare_exchange_weak(__pointer_type& __p1, __pointer_type __p2,
       memory_order __m1,
       memory_order __m2) volatile noexcept
      { return _M_b.compare_exchange_weak(__p1, __p2, __m1, __m2); }

      bool
      compare_exchange_weak(__pointer_type& __p1, __pointer_type __p2,
       memory_order __m = memory_order_seq_cst) noexcept
      {
 return compare_exchange_weak(__p1, __p2, __m,
         __cmpexch_failure_order(__m));
      }

      bool
      compare_exchange_weak(__pointer_type& __p1, __pointer_type __p2,
      memory_order __m = memory_order_seq_cst) volatile noexcept
      {
 return compare_exchange_weak(__p1, __p2, __m,
         __cmpexch_failure_order(__m));
      }

      bool
      compare_exchange_strong(__pointer_type& __p1, __pointer_type __p2,
         memory_order __m1, memory_order __m2) noexcept
      { return _M_b.compare_exchange_strong(__p1, __p2, __m1, __m2); }

      bool
      compare_exchange_strong(__pointer_type& __p1, __pointer_type __p2,
         memory_order __m1,
         memory_order __m2) volatile noexcept
      { return _M_b.compare_exchange_strong(__p1, __p2, __m1, __m2); }

      bool
      compare_exchange_strong(__pointer_type& __p1, __pointer_type __p2,
         memory_order __m = memory_order_seq_cst) noexcept
      {
 return _M_b.compare_exchange_strong(__p1, __p2, __m,
         __cmpexch_failure_order(__m));
      }

      bool
      compare_exchange_strong(__pointer_type& __p1, __pointer_type __p2,
      memory_order __m = memory_order_seq_cst) volatile noexcept
      {
 return _M_b.compare_exchange_strong(__p1, __p2, __m,
         __cmpexch_failure_order(__m));
      }
# 671 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/atomic" 3
      __pointer_type
      fetch_add(ptrdiff_t __d,
  memory_order __m = memory_order_seq_cst) noexcept
      {



 return _M_b.fetch_add(__d, __m);
      }

      __pointer_type
      fetch_add(ptrdiff_t __d,
  memory_order __m = memory_order_seq_cst) volatile noexcept
      {



 return _M_b.fetch_add(__d, __m);
      }

      __pointer_type
      fetch_sub(ptrdiff_t __d,
  memory_order __m = memory_order_seq_cst) noexcept
      {



 return _M_b.fetch_sub(__d, __m);
      }

      __pointer_type
      fetch_sub(ptrdiff_t __d,
  memory_order __m = memory_order_seq_cst) volatile noexcept
      {



 return _M_b.fetch_sub(__d, __m);
      }
    };



  template<>
    struct atomic<char> : __atomic_base<char>
    {
      typedef char __integral_type;
      typedef __atomic_base<char> __base_type;

      atomic() noexcept = default;
      ~atomic() noexcept = default;
      atomic(const atomic&) = delete;
      atomic& operator=(const atomic&) = delete;
      atomic& operator=(const atomic&) volatile = delete;

      constexpr atomic(__integral_type __i) noexcept : __base_type(__i) { }

      using __base_type::operator __integral_type;
      using __base_type::operator=;




    };


  template<>
    struct atomic<signed char> : __atomic_base<signed char>
    {
      typedef signed char __integral_type;
      typedef __atomic_base<signed char> __base_type;

      atomic() noexcept= default;
      ~atomic() noexcept = default;
      atomic(const atomic&) = delete;
      atomic& operator=(const atomic&) = delete;
      atomic& operator=(const atomic&) volatile = delete;

      constexpr atomic(__integral_type __i) noexcept : __base_type(__i) { }

      using __base_type::operator __integral_type;
      using __base_type::operator=;




    };


  template<>
    struct atomic<unsigned char> : __atomic_base<unsigned char>
    {
      typedef unsigned char __integral_type;
      typedef __atomic_base<unsigned char> __base_type;

      atomic() noexcept= default;
      ~atomic() noexcept = default;
      atomic(const atomic&) = delete;
      atomic& operator=(const atomic&) = delete;
      atomic& operator=(const atomic&) volatile = delete;

      constexpr atomic(__integral_type __i) noexcept : __base_type(__i) { }

      using __base_type::operator __integral_type;
      using __base_type::operator=;




    };


  template<>
    struct atomic<short> : __atomic_base<short>
    {
      typedef short __integral_type;
      typedef __atomic_base<short> __base_type;

      atomic() noexcept = default;
      ~atomic() noexcept = default;
      atomic(const atomic&) = delete;
      atomic& operator=(const atomic&) = delete;
      atomic& operator=(const atomic&) volatile = delete;

      constexpr atomic(__integral_type __i) noexcept : __base_type(__i) { }

      using __base_type::operator __integral_type;
      using __base_type::operator=;




    };


  template<>
    struct atomic<unsigned short> : __atomic_base<unsigned short>
    {
      typedef unsigned short __integral_type;
      typedef __atomic_base<unsigned short> __base_type;

      atomic() noexcept = default;
      ~atomic() noexcept = default;
      atomic(const atomic&) = delete;
      atomic& operator=(const atomic&) = delete;
      atomic& operator=(const atomic&) volatile = delete;

      constexpr atomic(__integral_type __i) noexcept : __base_type(__i) { }

      using __base_type::operator __integral_type;
      using __base_type::operator=;




    };


  template<>
    struct atomic<int> : __atomic_base<int>
    {
      typedef int __integral_type;
      typedef __atomic_base<int> __base_type;

      atomic() noexcept = default;
      ~atomic() noexcept = default;
      atomic(const atomic&) = delete;
      atomic& operator=(const atomic&) = delete;
      atomic& operator=(const atomic&) volatile = delete;

      constexpr atomic(__integral_type __i) noexcept : __base_type(__i) { }

      using __base_type::operator __integral_type;
      using __base_type::operator=;




    };


  template<>
    struct atomic<unsigned int> : __atomic_base<unsigned int>
    {
      typedef unsigned int __integral_type;
      typedef __atomic_base<unsigned int> __base_type;

      atomic() noexcept = default;
      ~atomic() noexcept = default;
      atomic(const atomic&) = delete;
      atomic& operator=(const atomic&) = delete;
      atomic& operator=(const atomic&) volatile = delete;

      constexpr atomic(__integral_type __i) noexcept : __base_type(__i) { }

      using __base_type::operator __integral_type;
      using __base_type::operator=;




    };


  template<>
    struct atomic<long> : __atomic_base<long>
    {
      typedef long __integral_type;
      typedef __atomic_base<long> __base_type;

      atomic() noexcept = default;
      ~atomic() noexcept = default;
      atomic(const atomic&) = delete;
      atomic& operator=(const atomic&) = delete;
      atomic& operator=(const atomic&) volatile = delete;

      constexpr atomic(__integral_type __i) noexcept : __base_type(__i) { }

      using __base_type::operator __integral_type;
      using __base_type::operator=;




    };


  template<>
    struct atomic<unsigned long> : __atomic_base<unsigned long>
    {
      typedef unsigned long __integral_type;
      typedef __atomic_base<unsigned long> __base_type;

      atomic() noexcept = default;
      ~atomic() noexcept = default;
      atomic(const atomic&) = delete;
      atomic& operator=(const atomic&) = delete;
      atomic& operator=(const atomic&) volatile = delete;

      constexpr atomic(__integral_type __i) noexcept : __base_type(__i) { }

      using __base_type::operator __integral_type;
      using __base_type::operator=;




    };


  template<>
    struct atomic<long long> : __atomic_base<long long>
    {
      typedef long long __integral_type;
      typedef __atomic_base<long long> __base_type;

      atomic() noexcept = default;
      ~atomic() noexcept = default;
      atomic(const atomic&) = delete;
      atomic& operator=(const atomic&) = delete;
      atomic& operator=(const atomic&) volatile = delete;

      constexpr atomic(__integral_type __i) noexcept : __base_type(__i) { }

      using __base_type::operator __integral_type;
      using __base_type::operator=;




    };


  template<>
    struct atomic<unsigned long long> : __atomic_base<unsigned long long>
    {
      typedef unsigned long long __integral_type;
      typedef __atomic_base<unsigned long long> __base_type;

      atomic() noexcept = default;
      ~atomic() noexcept = default;
      atomic(const atomic&) = delete;
      atomic& operator=(const atomic&) = delete;
      atomic& operator=(const atomic&) volatile = delete;

      constexpr atomic(__integral_type __i) noexcept : __base_type(__i) { }

      using __base_type::operator __integral_type;
      using __base_type::operator=;




    };


  template<>
    struct atomic<wchar_t> : __atomic_base<wchar_t>
    {
      typedef wchar_t __integral_type;
      typedef __atomic_base<wchar_t> __base_type;

      atomic() noexcept = default;
      ~atomic() noexcept = default;
      atomic(const atomic&) = delete;
      atomic& operator=(const atomic&) = delete;
      atomic& operator=(const atomic&) volatile = delete;

      constexpr atomic(__integral_type __i) noexcept : __base_type(__i) { }

      using __base_type::operator __integral_type;
      using __base_type::operator=;




    };
# 1016 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/atomic" 3
  template<>
    struct atomic<char16_t> : __atomic_base<char16_t>
    {
      typedef char16_t __integral_type;
      typedef __atomic_base<char16_t> __base_type;

      atomic() noexcept = default;
      ~atomic() noexcept = default;
      atomic(const atomic&) = delete;
      atomic& operator=(const atomic&) = delete;
      atomic& operator=(const atomic&) volatile = delete;

      constexpr atomic(__integral_type __i) noexcept : __base_type(__i) { }

      using __base_type::operator __integral_type;
      using __base_type::operator=;





    };


  template<>
    struct atomic<char32_t> : __atomic_base<char32_t>
    {
      typedef char32_t __integral_type;
      typedef __atomic_base<char32_t> __base_type;

      atomic() noexcept = default;
      ~atomic() noexcept = default;
      atomic(const atomic&) = delete;
      atomic& operator=(const atomic&) = delete;
      atomic& operator=(const atomic&) volatile = delete;

      constexpr atomic(__integral_type __i) noexcept : __base_type(__i) { }

      using __base_type::operator __integral_type;
      using __base_type::operator=;





    };



  typedef atomic<bool> atomic_bool;


  typedef atomic<char> atomic_char;


  typedef atomic<signed char> atomic_schar;


  typedef atomic<unsigned char> atomic_uchar;


  typedef atomic<short> atomic_short;


  typedef atomic<unsigned short> atomic_ushort;


  typedef atomic<int> atomic_int;


  typedef atomic<unsigned int> atomic_uint;


  typedef atomic<long> atomic_long;


  typedef atomic<unsigned long> atomic_ulong;


  typedef atomic<long long> atomic_llong;


  typedef atomic<unsigned long long> atomic_ullong;


  typedef atomic<wchar_t> atomic_wchar_t;







  typedef atomic<char16_t> atomic_char16_t;


  typedef atomic<char32_t> atomic_char32_t;






  typedef atomic<int8_t> atomic_int8_t;


  typedef atomic<uint8_t> atomic_uint8_t;


  typedef atomic<int16_t> atomic_int16_t;


  typedef atomic<uint16_t> atomic_uint16_t;


  typedef atomic<int32_t> atomic_int32_t;


  typedef atomic<uint32_t> atomic_uint32_t;


  typedef atomic<int64_t> atomic_int64_t;


  typedef atomic<uint64_t> atomic_uint64_t;



  typedef atomic<int_least8_t> atomic_int_least8_t;


  typedef atomic<uint_least8_t> atomic_uint_least8_t;


  typedef atomic<int_least16_t> atomic_int_least16_t;


  typedef atomic<uint_least16_t> atomic_uint_least16_t;


  typedef atomic<int_least32_t> atomic_int_least32_t;


  typedef atomic<uint_least32_t> atomic_uint_least32_t;


  typedef atomic<int_least64_t> atomic_int_least64_t;


  typedef atomic<uint_least64_t> atomic_uint_least64_t;



  typedef atomic<int_fast8_t> atomic_int_fast8_t;


  typedef atomic<uint_fast8_t> atomic_uint_fast8_t;


  typedef atomic<int_fast16_t> atomic_int_fast16_t;


  typedef atomic<uint_fast16_t> atomic_uint_fast16_t;


  typedef atomic<int_fast32_t> atomic_int_fast32_t;


  typedef atomic<uint_fast32_t> atomic_uint_fast32_t;


  typedef atomic<int_fast64_t> atomic_int_fast64_t;


  typedef atomic<uint_fast64_t> atomic_uint_fast64_t;



  typedef atomic<intptr_t> atomic_intptr_t;


  typedef atomic<uintptr_t> atomic_uintptr_t;


  typedef atomic<size_t> atomic_size_t;


  typedef atomic<ptrdiff_t> atomic_ptrdiff_t;


  typedef atomic<intmax_t> atomic_intmax_t;


  typedef atomic<uintmax_t> atomic_uintmax_t;


  inline bool
  atomic_flag_test_and_set_explicit(atomic_flag* __a,
        memory_order __m) noexcept
  { return __a->test_and_set(__m); }

  inline bool
  atomic_flag_test_and_set_explicit(volatile atomic_flag* __a,
        memory_order __m) noexcept
  { return __a->test_and_set(__m); }
# 1242 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/atomic" 3
  inline void
  atomic_flag_clear_explicit(atomic_flag* __a, memory_order __m) noexcept
  { __a->clear(__m); }

  inline void
  atomic_flag_clear_explicit(volatile atomic_flag* __a,
        memory_order __m) noexcept
  { __a->clear(__m); }

  inline bool
  atomic_flag_test_and_set(atomic_flag* __a) noexcept
  { return atomic_flag_test_and_set_explicit(__a, memory_order_seq_cst); }

  inline bool
  atomic_flag_test_and_set(volatile atomic_flag* __a) noexcept
  { return atomic_flag_test_and_set_explicit(__a, memory_order_seq_cst); }

  inline void
  atomic_flag_clear(atomic_flag* __a) noexcept
  { atomic_flag_clear_explicit(__a, memory_order_seq_cst); }

  inline void
  atomic_flag_clear(volatile atomic_flag* __a) noexcept
  { atomic_flag_clear_explicit(__a, memory_order_seq_cst); }
# 1289 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/atomic" 3
  template<typename _Tp>
    using __atomic_val_t = __type_identity_t<_Tp>;
  template<typename _Tp>
    using __atomic_diff_t = typename atomic<_Tp>::difference_type;




  template<typename _ITp>
    inline bool
    atomic_is_lock_free(const atomic<_ITp>* __a) noexcept
    { return __a->is_lock_free(); }

  template<typename _ITp>
    inline bool
    atomic_is_lock_free(const volatile atomic<_ITp>* __a) noexcept
    { return __a->is_lock_free(); }

  template<typename _ITp>
    inline void
    atomic_init(atomic<_ITp>* __a, __atomic_val_t<_ITp> __i) noexcept
    { __a->store(__i, memory_order_relaxed); }

  template<typename _ITp>
    inline void
    atomic_init(volatile atomic<_ITp>* __a, __atomic_val_t<_ITp> __i) noexcept
    { __a->store(__i, memory_order_relaxed); }

  template<typename _ITp>
    inline void
    atomic_store_explicit(atomic<_ITp>* __a, __atomic_val_t<_ITp> __i,
     memory_order __m) noexcept
    { __a->store(__i, __m); }

  template<typename _ITp>
    inline void
    atomic_store_explicit(volatile atomic<_ITp>* __a, __atomic_val_t<_ITp> __i,
     memory_order __m) noexcept
    { __a->store(__i, __m); }

  template<typename _ITp>
    inline _ITp
    atomic_load_explicit(const atomic<_ITp>* __a, memory_order __m) noexcept
    { return __a->load(__m); }

  template<typename _ITp>
    inline _ITp
    atomic_load_explicit(const volatile atomic<_ITp>* __a,
    memory_order __m) noexcept
    { return __a->load(__m); }

  template<typename _ITp>
    inline _ITp
    atomic_exchange_explicit(atomic<_ITp>* __a, __atomic_val_t<_ITp> __i,
        memory_order __m) noexcept
    { return __a->exchange(__i, __m); }

  template<typename _ITp>
    inline _ITp
    atomic_exchange_explicit(volatile atomic<_ITp>* __a,
        __atomic_val_t<_ITp> __i,
        memory_order __m) noexcept
    { return __a->exchange(__i, __m); }

  template<typename _ITp>
    inline bool
    atomic_compare_exchange_weak_explicit(atomic<_ITp>* __a,
       __atomic_val_t<_ITp>* __i1,
       __atomic_val_t<_ITp> __i2,
       memory_order __m1,
       memory_order __m2) noexcept
    { return __a->compare_exchange_weak(*__i1, __i2, __m1, __m2); }

  template<typename _ITp>
    inline bool
    atomic_compare_exchange_weak_explicit(volatile atomic<_ITp>* __a,
       __atomic_val_t<_ITp>* __i1,
       __atomic_val_t<_ITp> __i2,
       memory_order __m1,
       memory_order __m2) noexcept
    { return __a->compare_exchange_weak(*__i1, __i2, __m1, __m2); }

  template<typename _ITp>
    inline bool
    atomic_compare_exchange_strong_explicit(atomic<_ITp>* __a,
         __atomic_val_t<_ITp>* __i1,
         __atomic_val_t<_ITp> __i2,
         memory_order __m1,
         memory_order __m2) noexcept
    { return __a->compare_exchange_strong(*__i1, __i2, __m1, __m2); }

  template<typename _ITp>
    inline bool
    atomic_compare_exchange_strong_explicit(volatile atomic<_ITp>* __a,
         __atomic_val_t<_ITp>* __i1,
         __atomic_val_t<_ITp> __i2,
         memory_order __m1,
         memory_order __m2) noexcept
    { return __a->compare_exchange_strong(*__i1, __i2, __m1, __m2); }


  template<typename _ITp>
    inline void
    atomic_store(atomic<_ITp>* __a, __atomic_val_t<_ITp> __i) noexcept
    { atomic_store_explicit(__a, __i, memory_order_seq_cst); }

  template<typename _ITp>
    inline void
    atomic_store(volatile atomic<_ITp>* __a, __atomic_val_t<_ITp> __i) noexcept
    { atomic_store_explicit(__a, __i, memory_order_seq_cst); }

  template<typename _ITp>
    inline _ITp
    atomic_load(const atomic<_ITp>* __a) noexcept
    { return atomic_load_explicit(__a, memory_order_seq_cst); }

  template<typename _ITp>
    inline _ITp
    atomic_load(const volatile atomic<_ITp>* __a) noexcept
    { return atomic_load_explicit(__a, memory_order_seq_cst); }

  template<typename _ITp>
    inline _ITp
    atomic_exchange(atomic<_ITp>* __a, __atomic_val_t<_ITp> __i) noexcept
    { return atomic_exchange_explicit(__a, __i, memory_order_seq_cst); }

  template<typename _ITp>
    inline _ITp
    atomic_exchange(volatile atomic<_ITp>* __a,
      __atomic_val_t<_ITp> __i) noexcept
    { return atomic_exchange_explicit(__a, __i, memory_order_seq_cst); }

  template<typename _ITp>
    inline bool
    atomic_compare_exchange_weak(atomic<_ITp>* __a,
     __atomic_val_t<_ITp>* __i1,
     __atomic_val_t<_ITp> __i2) noexcept
    {
      return atomic_compare_exchange_weak_explicit(__a, __i1, __i2,
         memory_order_seq_cst,
         memory_order_seq_cst);
    }

  template<typename _ITp>
    inline bool
    atomic_compare_exchange_weak(volatile atomic<_ITp>* __a,
     __atomic_val_t<_ITp>* __i1,
     __atomic_val_t<_ITp> __i2) noexcept
    {
      return atomic_compare_exchange_weak_explicit(__a, __i1, __i2,
         memory_order_seq_cst,
         memory_order_seq_cst);
    }

  template<typename _ITp>
    inline bool
    atomic_compare_exchange_strong(atomic<_ITp>* __a,
       __atomic_val_t<_ITp>* __i1,
       __atomic_val_t<_ITp> __i2) noexcept
    {
      return atomic_compare_exchange_strong_explicit(__a, __i1, __i2,
           memory_order_seq_cst,
           memory_order_seq_cst);
    }

  template<typename _ITp>
    inline bool
    atomic_compare_exchange_strong(volatile atomic<_ITp>* __a,
       __atomic_val_t<_ITp>* __i1,
       __atomic_val_t<_ITp> __i2) noexcept
    {
      return atomic_compare_exchange_strong_explicit(__a, __i1, __i2,
           memory_order_seq_cst,
           memory_order_seq_cst);
    }
# 1495 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/atomic" 3
  template<typename _ITp>
    inline _ITp
    atomic_fetch_add_explicit(atomic<_ITp>* __a,
         __atomic_diff_t<_ITp> __i,
         memory_order __m) noexcept
    { return __a->fetch_add(__i, __m); }

  template<typename _ITp>
    inline _ITp
    atomic_fetch_add_explicit(volatile atomic<_ITp>* __a,
         __atomic_diff_t<_ITp> __i,
         memory_order __m) noexcept
    { return __a->fetch_add(__i, __m); }

  template<typename _ITp>
    inline _ITp
    atomic_fetch_sub_explicit(atomic<_ITp>* __a,
         __atomic_diff_t<_ITp> __i,
         memory_order __m) noexcept
    { return __a->fetch_sub(__i, __m); }

  template<typename _ITp>
    inline _ITp
    atomic_fetch_sub_explicit(volatile atomic<_ITp>* __a,
         __atomic_diff_t<_ITp> __i,
         memory_order __m) noexcept
    { return __a->fetch_sub(__i, __m); }

  template<typename _ITp>
    inline _ITp
    atomic_fetch_and_explicit(__atomic_base<_ITp>* __a,
         __atomic_val_t<_ITp> __i,
         memory_order __m) noexcept
    { return __a->fetch_and(__i, __m); }

  template<typename _ITp>
    inline _ITp
    atomic_fetch_and_explicit(volatile __atomic_base<_ITp>* __a,
         __atomic_val_t<_ITp> __i,
         memory_order __m) noexcept
    { return __a->fetch_and(__i, __m); }

  template<typename _ITp>
    inline _ITp
    atomic_fetch_or_explicit(__atomic_base<_ITp>* __a,
        __atomic_val_t<_ITp> __i,
        memory_order __m) noexcept
    { return __a->fetch_or(__i, __m); }

  template<typename _ITp>
    inline _ITp
    atomic_fetch_or_explicit(volatile __atomic_base<_ITp>* __a,
        __atomic_val_t<_ITp> __i,
        memory_order __m) noexcept
    { return __a->fetch_or(__i, __m); }

  template<typename _ITp>
    inline _ITp
    atomic_fetch_xor_explicit(__atomic_base<_ITp>* __a,
         __atomic_val_t<_ITp> __i,
         memory_order __m) noexcept
    { return __a->fetch_xor(__i, __m); }

  template<typename _ITp>
    inline _ITp
    atomic_fetch_xor_explicit(volatile __atomic_base<_ITp>* __a,
         __atomic_val_t<_ITp> __i,
         memory_order __m) noexcept
    { return __a->fetch_xor(__i, __m); }

  template<typename _ITp>
    inline _ITp
    atomic_fetch_add(atomic<_ITp>* __a,
       __atomic_diff_t<_ITp> __i) noexcept
    { return atomic_fetch_add_explicit(__a, __i, memory_order_seq_cst); }

  template<typename _ITp>
    inline _ITp
    atomic_fetch_add(volatile atomic<_ITp>* __a,
       __atomic_diff_t<_ITp> __i) noexcept
    { return atomic_fetch_add_explicit(__a, __i, memory_order_seq_cst); }

  template<typename _ITp>
    inline _ITp
    atomic_fetch_sub(atomic<_ITp>* __a,
       __atomic_diff_t<_ITp> __i) noexcept
    { return atomic_fetch_sub_explicit(__a, __i, memory_order_seq_cst); }

  template<typename _ITp>
    inline _ITp
    atomic_fetch_sub(volatile atomic<_ITp>* __a,
       __atomic_diff_t<_ITp> __i) noexcept
    { return atomic_fetch_sub_explicit(__a, __i, memory_order_seq_cst); }

  template<typename _ITp>
    inline _ITp
    atomic_fetch_and(__atomic_base<_ITp>* __a,
       __atomic_val_t<_ITp> __i) noexcept
    { return atomic_fetch_and_explicit(__a, __i, memory_order_seq_cst); }

  template<typename _ITp>
    inline _ITp
    atomic_fetch_and(volatile __atomic_base<_ITp>* __a,
       __atomic_val_t<_ITp> __i) noexcept
    { return atomic_fetch_and_explicit(__a, __i, memory_order_seq_cst); }

  template<typename _ITp>
    inline _ITp
    atomic_fetch_or(__atomic_base<_ITp>* __a,
      __atomic_val_t<_ITp> __i) noexcept
    { return atomic_fetch_or_explicit(__a, __i, memory_order_seq_cst); }

  template<typename _ITp>
    inline _ITp
    atomic_fetch_or(volatile __atomic_base<_ITp>* __a,
      __atomic_val_t<_ITp> __i) noexcept
    { return atomic_fetch_or_explicit(__a, __i, memory_order_seq_cst); }

  template<typename _ITp>
    inline _ITp
    atomic_fetch_xor(__atomic_base<_ITp>* __a,
       __atomic_val_t<_ITp> __i) noexcept
    { return atomic_fetch_xor_explicit(__a, __i, memory_order_seq_cst); }

  template<typename _ITp>
    inline _ITp
    atomic_fetch_xor(volatile __atomic_base<_ITp>* __a,
       __atomic_val_t<_ITp> __i) noexcept
    { return atomic_fetch_xor_explicit(__a, __i, memory_order_seq_cst); }
# 1796 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/atomic" 3

}
# 8 "./warn/Winvalid-memory-model-2.C" 2


# 9 "./warn/Winvalid-memory-model-2.C"
static const std::memory_order relaxed = std::memory_order_relaxed;
static const std::memory_order consume = std::memory_order_consume;
static const std::memory_order acquire = std::memory_order_acquire;
static const std::memory_order release = std::memory_order_release;
static const std::memory_order acq_rel = std::memory_order_acq_rel;
static const std::memory_order seq_cst = std::memory_order_seq_cst;

extern std::atomic<int> eai;

void test_load (int *pi)
{
  *pi++ = eai.load (relaxed);
  *pi++ = eai.load (consume);
  *pi++ = eai.load (acquire);
  *pi++ = eai.load (release);
  *pi++ = eai.load (acq_rel);
  *pi++ = eai.load (seq_cst);
}






void test_store (int *pi)
{
  eai.store (*pi++, relaxed);
  eai.store (*pi++, consume);
  eai.store (*pi++, acquire);
  eai.store (*pi++, release);
  eai.store (*pi++, acq_rel);
  eai.store (*pi++, seq_cst);
}







void test_exchange (const int *pi)
{
  eai.exchange (*pi++, relaxed);
  eai.exchange (*pi++, consume);
  eai.exchange (*pi++, acquire);
  eai.exchange (*pi++, release);
  eai.exchange (*pi++, acq_rel);
  eai.exchange (*pi++, seq_cst);
}


void test_compare_exchange (int *pi, int *pj)
{



  std::atomic_compare_exchange_weak_explicit (&eai, pi++, *pj++, relaxed, relaxed);
  std::atomic_compare_exchange_weak_explicit (&eai, pi++, *pj++, relaxed, consume);
  std::atomic_compare_exchange_weak_explicit (&eai, pi++, *pj++, relaxed, acquire);
  std::atomic_compare_exchange_weak_explicit (&eai, pi++, *pj++, relaxed, release);
  std::atomic_compare_exchange_weak_explicit (&eai, pi++, *pj++, relaxed, acq_rel);
  std::atomic_compare_exchange_weak_explicit (&eai, pi++, *pj++, relaxed, seq_cst);
  std::atomic_compare_exchange_weak_explicit (&eai, pi++, *pj++, relaxed, relaxed);







}
